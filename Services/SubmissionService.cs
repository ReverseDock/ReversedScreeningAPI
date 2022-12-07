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
    private readonly IUserFileRepository _userFileRepository;
    private readonly IReceptorFileService _receptorFileService;
    private readonly IDockingTaskPublisher _dockingPublisher;
    private readonly IDockingResultRepository _resultRepository;
    private readonly IConnectionMultiplexer _redis;
    private readonly IConfiguration _configuration;

    public SubmissionService(ILogger<SubmissionService> logger, ISubmissionRepository submissionRepository,
                             IUserFileRepository userFileRepository, IDockingTaskPublisher dockingPublisher,
                             IReceptorFileService receptorFileService, IDockingResultRepository resultRepository,
                             IConnectionMultiplexer redis, IConfiguration configuration)
    {
        _logger = logger;
        _submissionRepository = submissionRepository;
        _userFileRepository = userFileRepository;
        _dockingPublisher = dockingPublisher;
        _receptorFileService = receptorFileService;
        _resultRepository = resultRepository;
        _redis = redis;
        _configuration = configuration;
    }

    public async Task ConfirmSubmission(Guid submissionGuid)
    {
        var submission = await _submissionRepository.GetByGuid(submissionGuid);
        if (submission is null) throw new FileNotFoundException();

        submission.confirmed = true;
        await _submissionRepository.UpdateAsync(submission.id!, submission);
    }

    public async Task CreateDockings(string submissionId)
    {
        var submission = await _submissionRepository.GetAsync(submissionId);
        if (submission is null) throw new FileNotFoundException();
        var userFile = await _userFileRepository.GetAsync(submission.fileId!);
        if (userFile is null) throw new FileNotFoundException();

        var uniProtIds = await GetUniProtIdsFromSubmission(submissionId);
        var receptors = await _receptorFileService.GetFilesForUniProtIds(uniProtIds);
        foreach (var receptor in receptors)
        {
            var docking = new DockingTask
            {
                submissionId = submission.id!,
                receptorId = receptor.id!,
                fullLigandPath = userFile.fullPDBQTPath,
                fullReceptorPath = receptor.fullPDBQTPath,
                fullConfigPath = receptor.fullConfigPath
            };
            await _dockingPublisher.PublishDockingTask(docking);
        }
    }

    public async Task<Guid> CreateSubmission(Guid fileGuid, string emailAddress, string IP)
    {
        Guid guid = Guid.NewGuid();
        UserFile? userFile = await _userFileRepository.GetByGuid(fileGuid);
        if (userFile is null) throw new FileNotFoundException();
        // Add verification that no submission exists for file id

        await _submissionRepository.CreateAsync(new Submission {
            guid = guid,
            fileId = userFile.id,
            emailAddress = emailAddress,
            IP = IP,
            confirmed = false
        });
        return guid;
    }

    public async Task<string> CreateDirectory(Guid submissionGuid)
    {
        var directory = Path.Combine(_configuration.GetSection("Storage")["Submissions"], submissionGuid.ToString());
        var submission = await GetSubmission(submissionGuid);
        submission!.submissionPath = directory;
        await _submissionRepository.UpdateAsync(submission.id!, submission!);
        Directory.CreateDirectory(directory);
        return directory;
    }

    public async Task CreateReceptorListFile(Guid submissionGuid, string directory, IFormFile file)
    {
        try
        {
            var fileName = "receptors.txt";
            var fullPath = Path.Combine(directory, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            var submission = await GetSubmission(submissionGuid);
            submission!.receptorListPath = fullPath;
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

    public async Task<FileStream?> GetResultFile(Guid submissionGuid, Guid resultGuid)
    {
        var submission = await _submissionRepository.GetByGuid(submissionGuid);
        if (submission is null) throw new FileNotFoundException();
        var result = await _resultRepository.GetByGuid(resultGuid);
        if (result is null) throw new FileNotFoundException();

        var fileStream = new FileStream(result.fullOutputPath, FileMode.Open);
        return fileStream;
    }

    public async Task<HttpAPI.Models.DockingResult?> GetResult(Guid submissionGuid, Guid resultGuid)
    {
        return await _resultRepository.GetByGuid(resultGuid);
    }

    private async Task<IEnumerable<string>> GetUniProtIdsFromSubmission(string submissionId)
    {
        var submission = await _submissionRepository.GetAsync(submissionId);
        var uniProtIds = await File.ReadAllLinesAsync(submission!.receptorListPath);
        return uniProtIds;
    }
}