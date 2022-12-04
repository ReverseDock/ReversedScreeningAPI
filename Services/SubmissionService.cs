using HttpAPI.Models;
using HttpAPI.Models.DTO;
using AsyncAPI.Models;

using DataAccess.Repositories;
using AsyncAPI.Publishers;

using StackExchange.Redis;
using System.Text.Json;

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

    public SubmissionService(ILogger<SubmissionService> logger, ISubmissionRepository submissionRepository,
                             IUserFileRepository userFileRepository, IDockingTaskPublisher dockingPublisher,
                             IReceptorFileService receptorFileService, IDockingResultRepository resultRepository,
                             IConnectionMultiplexer redis)
    {
        _logger = logger;
        _submissionRepository = submissionRepository;
        _userFileRepository = userFileRepository;
        _dockingPublisher = dockingPublisher;
        _receptorFileService = receptorFileService;
        _resultRepository = resultRepository;
        _redis = redis;
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

        var receptors = await _receptorFileService.GetFiles();
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
}