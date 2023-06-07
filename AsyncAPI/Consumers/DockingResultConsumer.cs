using AsyncAPI.Models;

using DataAccess.Repositories;

using Services;

using MassTransit;
using HttpAPI.Models;
using AsyncAPI.Publishers;

namespace AsyncAPI.Consumers;

public class DockingResultConsumer : IConsumer<AsyncAPI.Models.DockingResult>
{
    private readonly ILogger<DockingResultConsumer> _logger;
    private readonly IDockingResultRepository _DockingResultsRepository;
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IFileService _fileService;
    private readonly ISubmissionService _submissionService;
    private readonly IMailService _mailService;

    public DockingResultConsumer(ILogger<DockingResultConsumer> logger, IDockingResultRepository DockingResultRepository, IFileService fileService,
                                 ISubmissionRepository submissionRepository, ISubmissionService submissionService,
                                 IMailService mailService)
    {
        _logger = logger;
        _DockingResultsRepository = DockingResultRepository;
        _fileService = fileService;
        _submissionRepository = submissionRepository;
        _submissionService = submissionService;
        _mailService = mailService;
    }

    public async Task Consume(ConsumeContext<AsyncAPI.Models.DockingResult> context)
    {
        var model = context.Message;
        FileDescriptor? outputFile = null;
        if (model.outputPath != "")
            outputFile = await _fileService.CreateFile(model.outputPath, "results", true);
        var submission = await _submissionRepository.GetAsync(model.submission);
        if (submission is null)
        {
            _logger.LogError($"Could not find submission of DockingResult. Submission: {model.submission}");
            return;
        }

        if (submission.status != SubmissionStatus.InProgress)
        {
            submission.status = SubmissionStatus.InProgress;
            await _submissionRepository.UpdateAsync(submission.id!, submission);
        }

        var dbDockingResult = await _DockingResultsRepository.GetBySubmissionIdAndReceptorId(model.submission, model.receptor);
        if (dbDockingResult is null)
        {
            _logger.LogError($"Could not find DockingResult for message. Submission: {model.submission}, Receptor: {model.receptor}");
            return;
        }
    
        dbDockingResult.affinity = model.affinity;
        dbDockingResult.outputFileId = outputFile?.id!;
        dbDockingResult.secondsToCompletion = model.secondsToCompletion != -1 ? model.secondsToCompletion : 0;
        dbDockingResult.success = model.success;
        await _DockingResultsRepository.UpdateAsync(dbDockingResult.id!, dbDockingResult);

        var progress = await _submissionService.GetProgress(submission);
        if (progress == 1.0)
        {
            submission!.status = SubmissionStatus.Finished;
            await _mailService.PublishFinishedMail(submission!);
        }
        await _submissionRepository.UpdateAsync(submission.id!, submission);
    }
}