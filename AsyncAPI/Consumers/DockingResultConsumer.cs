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
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IFileService _fileService;
    private readonly ISubmissionService _submissionService;
    private readonly IMailService _mailService;

    public DockingResultConsumer(ILogger<DockingResultConsumer> logger, IFileService fileService,
                                 ISubmissionRepository submissionRepository, ISubmissionService submissionService,
                                 IMailService mailService)
    {
        _logger = logger;
        _fileService = fileService;
        _submissionRepository = submissionRepository;
        _submissionService = submissionService;
        _mailService = mailService;
    }

    public async Task Consume(ConsumeContext<DockingResult> context)
    {
        var model = context.Message;
        FileDescriptor? outputFile = null;
        if (model.outputPath != "")
            outputFile = _fileService.CreateFile(model.outputPath, "results", true);
        var submission = await _submissionRepository.GetAsync(model.submission);
        if (submission is null)
        {
            _logger.LogError($"Could not find submission of DockingResult. Submission: {model.submission}");
            return;
        }

        // Assumes single-threaded consuming of messages
        if (submission.status != SubmissionStatus.InProgress)
        {
            submission.status = SubmissionStatus.InProgress;
            await _submissionRepository.UpdateAsync(submission.id!, submission);
        }

        var receptor = submission.receptors.Find(r => r.guid == model.receptor)!;
    
        receptor.affinity = model.success ? model.affinity : -1;
        receptor.outputFile = outputFile;
        receptor.secondsToCompletion = model.secondsToCompletion != -1 ? model.secondsToCompletion : 0;
        receptor.success = model.success;

        await _submissionRepository.UpdateReceptor(submission, receptor);

        // Get updated submission
        submission = await _submissionRepository.GetAsync(submission.id!);

        var progress = _submissionService.GetProgress(submission!);
        if (progress == 1.0)
        {
            submission!.status = SubmissionStatus.Finished;
            await _mailService.PublishFinishedMail(submission!);
            await _submissionRepository.UpdateAsync(submission.id!, submission);
        }

    }
}