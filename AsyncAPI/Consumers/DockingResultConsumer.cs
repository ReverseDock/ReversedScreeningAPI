using AsyncAPI.Models;

using DataAccess.Repositories;

using Services;

using MassTransit;
using HttpAPI.Models;
using AsyncAPI.Publishers;

namespace AsyncAPI.Consumers;

public class DockingResultConsumer : IConsumer<AsyncAPI.Models.DockingResult>
{
    private readonly IDockingResultRepository _DockingResultsRepository;
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IFileService _fileService;
    private readonly ISubmissionService _submissionService;
    private readonly IMailService _mailService;

    public DockingResultConsumer(IDockingResultRepository DockingResultRepository, IFileService fileService,
                                 ISubmissionRepository submissionRepository, ISubmissionService submissionService,
                                 IMailService mailService)
    {
        _DockingResultsRepository = DockingResultRepository;
        _fileService = fileService;
        _submissionRepository = submissionRepository;
        _submissionService = submissionService;
        _mailService = mailService;
    }

    public async Task Consume(ConsumeContext<AsyncAPI.Models.DockingResult> context)
    {
        var model = context.Message;
        var outputFile = await _fileService.CreateFile(model.outputPath, true);
        var submission = await _submissionRepository.GetAsync(model.submission);

        if (submission!.status != SubmissionStatus.InProgress)
        {
            submission!.status = SubmissionStatus.InProgress;
            await _submissionRepository.UpdateAsync(submission.id!, submission);
        }

        var dbDockingResult = new HttpAPI.Models.DockingResult
        {
            guid = Guid.NewGuid(),
            submissionId = model.submission,
            receptorId = model.receptor,
            affinity = model.affinity,
            outputFileId = outputFile!.id!,
            secondsToCompletion = model.secondsToCompletion,
            success = model.success
        };
        await _DockingResultsRepository.CreateAsync(dbDockingResult);

        var progress = await _submissionService.GetProgress(submission!.guid);
        if (progress == 1.0)
        {
            submission!.status = SubmissionStatus.Finished;
            await _mailService.PublishFinishedMail(submission!);
        }
        await _submissionRepository.UpdateAsync(submission.id!, submission);
    }
}