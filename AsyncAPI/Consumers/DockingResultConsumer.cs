using AsyncAPI.Models;

using DataAccess.Repositories;

using Services;

using MassTransit;

namespace AsyncAPI.Consumers;

public class DockingResultConsumer : IConsumer<AsyncAPI.Models.DockingResult>
{
    private readonly IDockingResultRepository _DockingResultsRepository;
    private readonly IFileService _fileService;

    public DockingResultConsumer(IDockingResultRepository DockingResultRepository, IFileService fileService)
    {
        _DockingResultsRepository = DockingResultRepository;
        _fileService = fileService;
    }

    public async Task Consume(ConsumeContext<AsyncAPI.Models.DockingResult> context)
    {
        var model = context.Message;
        var outputFile = await _fileService.CreateFile(model.outputPath, true);
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