using HttpAPI.Models;
using AsyncAPI.Models;

using DataAccess.Repositories;
using AsyncAPI.Publishers;

namespace HttpAPI.Services;

public class SubmissionService : ISubmissionService
{
    private readonly ILogger<SubmissionService> _logger;
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IUserFileRepository _userFileRepository;
    private readonly IReceptorFileService _receptorFileService;
    private readonly IDockingPublisher _dockingPublisher;
    private readonly IResultRepository _resultRepository;
    
    public SubmissionService(ILogger<SubmissionService> logger, ISubmissionRepository submissionRepository,
                             IUserFileRepository userFileRepository, IDockingPublisher dockingPublisher,
                             IReceptorFileService receptorFileService, IResultRepository resultRepository)
    {
        _logger = logger;
        _submissionRepository = submissionRepository;
        _userFileRepository = userFileRepository;
        _dockingPublisher = dockingPublisher;
        _receptorFileService = receptorFileService;
        _resultRepository = resultRepository;
    }

    public async Task ConfirmSubmission(Guid submissionGuid)
    {
        var submission = await _submissionRepository.GetByGuid(submissionGuid);
        if (submission is null) throw new FileNotFoundException();

        submission.confirmed = true;
        await _submissionRepository.UpdateAsync(submission.id!, submission);
    }

    public async Task CreateDockings(Guid submissionGuid)
    {
        var submission = await _submissionRepository.GetByGuid(submissionGuid);
        if (submission is null) throw new FileNotFoundException();
        var userFile = await _userFileRepository.GetAsync(submission.fileId!);
        if (userFile is null) throw new FileNotFoundException();

        var receptors = await _receptorFileService.GetFiles();
        foreach (var receptor in receptors)
        {
            var docking = new Docking
            {
                submissionId = submission.id!,
                receptorId = receptor.id!,
                fullLigandPath = userFile.fullPath,
                fullReceptorPath = receptor.fullPath
            };
            await _dockingPublisher.PublishDocking(docking);
        }
    }

    public async Task<Guid> CreateSubmission(Guid fileGuid, string emailAddress, string IP)
    {
        Guid guid = Guid.NewGuid();
        UserFile? userFile = await _userFileRepository.GetByGuid(fileGuid);
        if (userFile is null) throw new FileNotFoundException();
        // Add verification that no submission exists

        await _submissionRepository.CreateAsync(new Submission {
            guid = guid,
            fileId = userFile.id,
            emailAddress = emailAddress,
            IP = IP,
            confirmed = false
        });
        return guid;
    }

    public async Task<List<Models.Result>> GetResults(Guid submissionGuid)
    {
        var submission = await _submissionRepository.GetByGuid(submissionGuid);
        if (submission is null) throw new FileNotFoundException();
        return await _resultRepository.GetBySubmissionId(submission.id!);
    }
}