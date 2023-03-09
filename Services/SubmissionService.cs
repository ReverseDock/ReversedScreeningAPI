using HttpAPI.Models;
using HttpAPI.Models.DTO;
using AsyncAPI.Models;

using DataAccess.Repositories;
using AsyncAPI.Publishers;

using StackExchange.Redis;
using System.Text.RegularExpressions;

namespace Services;

public class SubmissionService : ISubmissionService
{
    private readonly ILogger<SubmissionService> _logger;
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IReceptorService _receptorService;
    private readonly IDockingTaskPublisher _dockingPublisher;
    private readonly IDockingResultRepository _resultRepository;
    private readonly IFileService _fileService;
    private readonly IConnectionMultiplexer _redis;
    private readonly IConfiguration _configuration;

    public SubmissionService(ILogger<SubmissionService> logger, ISubmissionRepository submissionRepository,
                             IDockingTaskPublisher dockingPublisher, IFileService fileService,
                             IReceptorService receptorService, IDockingResultRepository resultRepository,
                             IConnectionMultiplexer redis, IConfiguration configuration)
    {
        _logger = logger;
        _submissionRepository = submissionRepository;
        _dockingPublisher = dockingPublisher;
        _fileService = fileService;
        _receptorService = receptorService;
        _resultRepository = resultRepository;
        _redis = redis;
        _configuration = configuration;
    }

    public async Task ConfirmSubmission(Submission submission)
    {
        submission.status = SubmissionStatus.Confirmed;
        await _submissionRepository.UpdateAsync(submission.id!, submission);
    }

    public async Task CreateDockings(Submission submission)
    {
        var ligandFile = await _fileService.GetFile(submission.pdbqtFileId!);
        if (ligandFile is null) throw new FileNotFoundException($"Ligand PDBQT file not found. Submission {submission.guid}");

        var uniProtIds = await GetUniProtIdsFromSubmission(submission);
        var receptors = await _receptorService.GetReceptorsForUniProtIds(uniProtIds);
        foreach (var receptor in receptors)
        {
            if (receptor.status != ReceptorFileStatus.Ready) continue;
            var receptorFile = await _fileService.GetFile(receptor.pdbqtFileId!);
            if (receptorFile == null) continue;
            var receptorConfig = await _fileService.GetFile(receptor.configFileId!);
            if (receptorConfig == null) continue;
            var docking = new DockingTask
            {
                submissionId = submission.id!,
                receptorId = receptor.id!,
                ligandPath = ligandFile.path,
                receptorPath = receptorFile.path,
                configPath = receptorConfig.path,
                exhaustiveness = submission.exhaustiveness
            };
            await _dockingPublisher.PublishDockingTask(docking);
        }
    }

    public async Task<Submission> CreateSubmission(IFormFile ligandFile, string ipAddress)
    {
        Guid guid = Guid.NewGuid();
        Guid confirmationGuid = Guid.NewGuid();

        var file = await _fileService.CreateFile(ligandFile, "ligands", true);

        var submission = await _submissionRepository.CreateAsync(new Submission {
            guid = guid,
            confirmationGuid = confirmationGuid,
            fileId = file!.id,
            IP = ipAddress,
            exhaustiveness = 0
        });

        return submission;
    }

    public async Task<FileDescriptor?> AddReceptors(Submission submission, IFormFile file)
    {
        if (submission.receptorListFileId != null)
            await _fileService.RemoveFile(submission.receptorListFileId);

        var fileDescriptor = await _fileService.CreateFile(file, "receptorlists", false);

        if (!(await ValidateReceptorsFile(fileDescriptor))) return null;

        submission.receptorListFileId = fileDescriptor.id;
        submission.status = SubmissionStatus.ConfirmationPending;
        await _submissionRepository.UpdateAsync(submission.id!, submission);
        return fileDescriptor;
    }

    public async Task<List<DockingResultDTO>> GetResults(Submission submission)
    {
        var results = await _resultRepository.GetDTOAsync(submission.id!);
        return results;
    }

    public async Task<Submission?> GetSubmission(string submissionId)
    {
        var submission = await _submissionRepository.GetAsync(submissionId);
        return submission;
    }

    public async Task<Submission?> GetSubmission(Guid submissionGuid)
    {
        var submission = await _submissionRepository.GetByGuid(submissionGuid);
        return submission;
    }

    public async Task<FileDescriptor?> GetResultFile(Submission submission, Guid resultGuid)
    {
        var result = await _resultRepository.GetByGuid(resultGuid);
        if (result is null || result.submissionId != submission.id) return null;
        return await _fileService.GetFile(result.outputFileId);
    }

    public async Task<HttpAPI.Models.DockingResult?> GetResult(Submission submission, Guid resultGuid)
    {
        return await _resultRepository.GetByGuid(resultGuid);
    }

    public async Task<IEnumerable<string>> GetUniProtIdsFromSubmission(Submission submission)
    {
        var file = await _fileService.GetFile(submission.receptorListFileId!);
        if (file is null) throw new FileNotFoundException();
        var uniProtIds = await File.ReadAllLinesAsync(file.path);

        var filteredUniProtIds = uniProtIds
            .Where(x =>
            {
                if (x.Contains(":"))
                {
                    var elements = x.Split(":");
                    if (elements[0] == "UniProtKB") return true;
                    return false;
                }
                return true;
            })
            .Select(x =>
            {
                if (x.Contains(":"))
                {
                    var elements = x.Split(":");
                    var elements2 = elements[1].Split('\t');
                    return elements2[0];
                }
                return x;
            });

        return filteredUniProtIds.Distinct();
    }

    public async Task<List<Submission>> GetSubmissions()
    {
        return await _submissionRepository.GetAsync();
    }

    public async Task UpdateSubmission(Submission submission)
    {
        await _submissionRepository.UpdateAsync(submission.id!, submission);
    }

    public async Task<float> GetProgress(Submission submission)
    {
        var uniProtIds = await GetUniProtIdsFromSubmission(submission);
        var results = await _resultRepository.GetBySubmissionId(submission.id!);
        var receptors = await _receptorService.GetReceptorsForUniProtIds(uniProtIds);
        var numberOfOkayReceptors = receptors.Count(rec => rec.status == ReceptorFileStatus.Ready);
        if (numberOfOkayReceptors == 0) 
        {
            _logger.LogWarning($"Submission without any 'Okay' receptors. Guid: {submission.guid}");
            return 1;
        }
        return (float) results.Count() / (float) numberOfOkayReceptors;
    }

    public async Task<SubmissionStatus?> GetStatus(Submission submission)
    {
        await Task.CompletedTask;
        return submission.status;
    }

    public async Task<bool> ValidateReceptorsFile(FileDescriptor file)
    {
        var lines = await File.ReadAllLinesAsync(file.path);

        string uniProtIdPattern = @"^[\w]+$";
        return !lines.Any(x =>
        {
            var trimmed = x.Trim();

            if (trimmed == "") return false;

            if (trimmed.Contains(":"))
            {
                var elements = trimmed.Split(":");
                if (elements[0] == "UniProtKB")
                {
                    var elements2 = elements[1].Split('\t');
                    var uniProtId = elements2[0];
                    return !Regex.Match(uniProtId, uniProtIdPattern).Success;
                }
                return false;
            }
            else
            {
                return !Regex.Match(trimmed, uniProtIdPattern).Success;
            }
        });
    }

    public async Task<bool> CheckReceptorsCount(Submission submission)
    {
        var receptorsDTO = await GetReceptorDTOs(submission);

        var maxReceptors = int.Parse(_configuration.GetSection("Limitations")["MaxReceptorAmount"]);
        var okayReceptors = receptorsDTO.Where(x => x.status == "Okay").Count();

        if (okayReceptors > maxReceptors)
        {
            return false;
        }
        return true;
    }

    public async Task<int> CalculateAndSetExhaustiveness(Submission submission)
    {
        var receptorsDTO = await GetReceptorDTOs(submission);

        var maxReceptors = int.Parse(_configuration.GetSection("Limitations")["MaxReceptorAmount"]);
        var okayReceptors = receptorsDTO.Where(x => x.status == "Okay").Count();

        var maxExhaustiveness = int.Parse(_configuration.GetSection("Limitations")["MaxExhaustiveness"]);
        var exhaustiveness = (int) Math.Ceiling((((float) (maxReceptors - okayReceptors + 1)) / (float) (maxReceptors)) * maxExhaustiveness);
        submission.exhaustiveness = exhaustiveness;
        await UpdateSubmission(submission); 
        return exhaustiveness;
    }

    public async Task<IEnumerable<ReceptorDTO>> GetReceptorDTOs(Submission submission)
    {
        var uniProtIds = await GetUniProtIdsFromSubmission(submission);
        var receptorsDTO = await _receptorService.GetReceptorDTOs(uniProtIds);
        return receptorsDTO;
    }

    public async Task<IEnumerable<Receptor>> GetUnprocessedReceptors(Submission submission)
    {
        var uniProtIds = await GetUniProtIdsFromSubmission(submission);
        var receptors = await _receptorService.GetReceptorsForUniProtIds(uniProtIds); 
        return receptors.Where(x => x.status == ReceptorFileStatus.Unprocessed);
    }
}