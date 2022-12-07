using HttpAPI.Models;
using HttpAPI.Models.DTO;
using AsyncAPI.Models;

using DataAccess.Repositories;
using AsyncAPI.Publishers;

using StackExchange.Redis;
using System.Text.Json;
using System.Net.Http.Headers;

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

    public async Task ConfirmSubmission(Guid submissionGuid, Guid confirmationGuid)
    {
        var submission = await _submissionRepository.GetByGuid(submissionGuid);
        if (submission is null) throw new FileNotFoundException();
        if (submission.confirmationGuid == confirmationGuid
            && submission.status != SubmissionStatus.ConfirmationPending)
            submission.status = SubmissionStatus.Confirmed;
        else
            return;
        await _submissionRepository.UpdateAsync(submission.id!, submission);
    }

    public async Task CreateDockings(string submissionId)
    {
        var submission = await _submissionRepository.GetAsync(submissionId);
        if (submission is null) throw new FileNotFoundException();
        var ligandFile = await _fileService.GetFile(submission!.pdbqtFileId!);
        if (ligandFile is null) throw new FileNotFoundException();

        var uniProtIds = await GetUniProtIdsFromSubmission(submissionId);
        var receptors = await _receptorService.GetReceptorsForUniProtIds(uniProtIds);
        foreach (var receptor in receptors)
        {
            if (receptor.status != ReceptorFileStatus.Ready) continue;
            var receptorFile = await _fileService.GetFile(receptor.pdbqtFileId!);
            var receptorConfig = await _fileService.GetFile(receptor.configFileId!);
            var docking = new DockingTask
            {
                submissionId = submission.id!,
                receptorId = receptor.id!,
                ligandPath = ligandFile.path,
                receptorPath = receptorFile!.path,
                configPath = receptorConfig!.path
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
            IP = ipAddress
        });

        return submission;
    }

    public async Task AddReceptors(Guid submissionGuid, IFormFile file)
    {
        var submission = await GetSubmission(submissionGuid);
        if (submission!.receptorListFileId != null)
            await _fileService.RemoveFile(submission!.receptorListFileId);
        try
        {
            var fileDescriptor = await _fileService.CreateFile(file, "receptorlists", false);
            
            submission!.receptorListFileId = fileDescriptor!.id;
            await _submissionRepository.UpdateAsync(submission.id!, submission!);

            return;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error when creating file: {ex}");
            return;
        }
    }

    public async Task<List<DockingResultDTO>> GetResults(Guid submissionGuid)
    {
        var submission = await _submissionRepository.GetByGuid(submissionGuid);
        if (submission is null) throw new FileNotFoundException();
        var results = await _resultRepository.GetDTOAsync(submission.id!);

        return results;
    }

    public async Task<Submission?> GetSubmission(Guid submissionGuid)
    {
        var submission = await _submissionRepository.GetByGuid(submissionGuid);
        if (submission is null) throw new FileNotFoundException();

        return submission;
    }

    public async Task<FileDescriptor?> GetResultFile(Guid submissionGuid, Guid resultGuid)
    {
        var submission = await _submissionRepository.GetByGuid(submissionGuid);
        if (submission is null) throw new FileNotFoundException();
        var result = await _resultRepository.GetByGuid(resultGuid);
        if (result is null) throw new FileNotFoundException();
        if (result.submissionId != submission.id) throw new FileNotFoundException();
        return await _fileService.GetFile(result.outputFileId);
    }

    public async Task<HttpAPI.Models.DockingResult?> GetResult(Guid submissionGuid, Guid resultGuid)
    {
        return await _resultRepository.GetByGuid(resultGuid);
    }

    public async Task<IEnumerable<string>> GetUniProtIdsFromSubmission(string submissionId)
    {
        var submission = await _submissionRepository.GetAsync(submissionId);
        var file = await _fileService.GetFile(submission!.receptorListFileId!);
        var uniProtIds = await File.ReadAllLinesAsync(file!.path);
        return uniProtIds;
    }
}