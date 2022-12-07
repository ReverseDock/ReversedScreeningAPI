using AsyncAPI.Models;

using DataAccess.Repositories;

using HttpAPI.Models;

using MassTransit;

namespace AsyncAPI.Consumers;

public class DockingResultConsumer : IConsumer<AsyncAPI.Models.DockingResult>
{
    private readonly IDockingResultRepository _DockingResultsRepository;

    public DockingResultConsumer(IDockingResultRepository DockingResultRepository)
    {
        _DockingResultsRepository = DockingResultRepository;
    }

    public async Task Consume(ConsumeContext<AsyncAPI.Models.DockingResult> context)
    {
        var model = context.Message;
        var dbDockingResult = new HttpAPI.Models.DockingResult
        {
            guid = Guid.NewGuid(),
            submissionId = model.submission,
            receptorId = model.receptor,
            affinity = model.affinity,
            fullOutputPath = model.fullOutputPath,
            secondsToCompletion = model.secondsToCompletion
        };
        await _DockingResultsRepository.CreateAsync(dbDockingResult);
    }
}