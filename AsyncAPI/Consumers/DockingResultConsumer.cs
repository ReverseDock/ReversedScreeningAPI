using AsyncAPI.Models;

using DataAccess.Repositories;

using Services;

using MassTransit;
using HttpAPI.Models;

namespace AsyncAPI.Consumers;

public class DockingResultConsumer : IConsumer<AsyncAPI.Models.DockingResult>
{
    private readonly IDockingResultRepository _DockingResultsRepository;
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IFileService _fileService;

    public DockingResultConsumer(IDockingResultRepository DockingResultRepository, IFileService fileService,
                                 ISubmissionRepository submissionRepository)
    {
        _DockingResultsRepository = DockingResultRepository;
        _fileService = fileService;
        _submissionRepository = submissionRepository;
    }

    public async Task Consume(ConsumeContext<AsyncAPI.Models.DockingResult> context)
    {
        var model = context.Message;
        var outputFile = await _fileService.CreateFile(model.outputPath, true);
        var submission = await _submissionRepository.GetAsync(model.submission);
        submission!.status = SubmissionStatus.InProgress;
        await _submissionRepository.UpdateAsync(submission.id!, submission);
        var dbDockingResult = new HttpAPI.Models.DockingResult
        {
            guid = Guid.NewGuid(),
            submissionId = model.submission,
            receptorId = model.receptor,
            affinity = model.affinity,
            outputFileId = outputFile!.id!,
            secondsToCompletion = model.secondsToCompletion
        };
        await _DockingResultsRepository.CreateAsync(dbDockingResult);
    }
}