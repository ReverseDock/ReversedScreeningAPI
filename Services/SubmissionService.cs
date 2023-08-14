using HttpAPI.Models;
using HttpAPI.Models.DTO;
using AsyncAPI.Models;
using MongoDB.Bson;

using DataAccess.Repositories;
using AsyncAPI.Publishers;

using StackExchange.Redis;

namespace Services;

public class SubmissionService : ISubmissionService
{
    private readonly ILogger<SubmissionService> _logger;
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IDockingTaskPublisher _dockingPublisher;
    private readonly IFileService _fileService;
    private readonly IConfiguration _configuration;

    public SubmissionService(ILogger<SubmissionService> logger, ISubmissionRepository submissionRepository,
                             IDockingTaskPublisher dockingPublisher, IFileService fileService,
                             IConfiguration configuration)
    {
        _logger = logger;
        _submissionRepository = submissionRepository;
        _dockingPublisher = dockingPublisher;
        _fileService = fileService;
        _configuration = configuration;
    }

    public async Task ConfirmSubmission(Submission submission)
    {
        submission.status = SubmissionStatus.Confirmed;
        await _submissionRepository.UpdateAsync(submission.id!, submission);
    }

    public async Task CreateDockings(Submission submission)
    {
        var ligandFile = submission.pdbqtLigand;

        if (ligandFile == null)
        {
            _logger.LogError($"Submission {submission.id} has no ligand file");
            return;
        }

        var receptors = submission.receptors;

        var exhaustiveness = int.Parse(_configuration.GetSection("Limitations")["Exhaustiveness"]);

        foreach (var receptor in receptors)
        {
            if (receptor.status != ReceptorFileStatus.Ready) continue;
            var receptorFile = receptor.pdbqtFile;
            if (receptorFile == null) continue;
            var receptorConfig =  receptor.configFile;
            if (receptorConfig == null) continue;
            var docking = new DockingTask
            {
                submissionId = submission.id!,
                receptorId = receptor.guid!,
                ligandPath = ligandFile!.path,
                receptorPath = receptorFile.path,
                configPath = receptorConfig.path,
                exhaustiveness = exhaustiveness
            };

            await _dockingPublisher.PublishDockingTask(docking);
        }
    }

    public async Task<int> GetUnfinishedDockingsCount()
    {
        var submissions = await _submissionRepository.GetAsync();
        var count = 0;
        foreach (var submission in submissions)
        {
            if (submission.status != SubmissionStatus.InProgress) continue;
            count += submission.receptors.Count(r => r.status == ReceptorFileStatus.Ready);
        }

        return count;
    }

    public async Task<Submission> CreateSubmission(IFormFile ligandFile, string ipAddress)
    {
        Guid guid = Guid.NewGuid();
        Guid confirmationGuid = Guid.NewGuid();

        var file = _fileService.CreateFile(ligandFile, "ligands", true);

        var submission = await _submissionRepository.CreateAsync(new Submission {
            guid = guid,
            confirmationGuid = confirmationGuid,
            IP = ipAddress,
            ligand = file
        });

        return submission;
    }

    public async Task AddReceptors(Submission submission, IFormFileCollection files)
    {
        var receptors = new List<Receptor>();
        
        foreach (var file in files)
        {
            _logger.LogInformation($"Adding receptor {file.FileName} to submission {submission.id}");
            var fileDescriptor = _fileService.CreateFile(file, "receptors", true);
            if (fileDescriptor is null) continue;
            receptors.Add(new Receptor
            {
                name = file.FileName,
                status = ReceptorFileStatus.Unprocessed,
                file = fileDescriptor,
                guid = Guid.NewGuid(), 
                secondsToCompletion = -1
            });

        }

        submission.receptors = receptors;
        submission.status = SubmissionStatus.ConfirmationPending;
        await _submissionRepository.UpdateAsync(submission.id!, submission);
    }

    public List<DockingResultDTO> GetResults(Submission submission)
    {
        var results = new List<DockingResultDTO>();
        foreach (var receptor in submission.receptors)
        {
            var receptorResult = new DockingResultDTO
            {
                guid = receptor.guid,
                receptorFASTA = receptor.FASTA,
                receptorName = receptor.name,
                affinity = receptor.affinity,
                success = receptor.success,
                status = receptor.status
            };
            results.Add(receptorResult);
        }

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
        var receptor = await _submissionRepository.GetReceptor(submission, resultGuid);
        if (receptor is null) return null;
        return receptor.outputFile;
    }

    public async Task<List<Submission>> GetSubmissions()
    {
        return await _submissionRepository.GetAsync();
    }

    public async Task UpdateSubmission(Submission submission)
    {
        await _submissionRepository.UpdateAsync(submission.id!, submission);
    }

    public float GetProgress(Submission submission)
    {
        var results = submission.receptors.Where(res => res.secondsToCompletion != -1);
        var numberOfOkayReceptors = submission.receptors.Count(res => res.status == ReceptorFileStatus.Ready);
        if (numberOfOkayReceptors == 0) 
        {
            _logger.LogWarning($"Submission without any 'Okay' receptors. Guid: {submission.guid}");
            return 1;
        }
        return results.Count() / (float) numberOfOkayReceptors;
    }

    public async Task<SubmissionStatus?> GetStatus(Submission submission)
    {
        await Task.CompletedTask;
        return submission.status;
    }

    public async Task<IEnumerable<Receptor>> GetUnprocessedReceptors(Submission submission)
    {
        return submission.receptors.Where(x => x.status == ReceptorFileStatus.Unprocessed);
    }
}