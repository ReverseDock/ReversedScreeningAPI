using AsyncAPI.Models;

using DataAccess.Repositories;

using HttpAPI.Models;

using MassTransit;

namespace AsyncAPI.Consumers;

public class ResultConsumer : IConsumer<AsyncAPI.Models.Result>
{
    private readonly IResultRepository _resultsRepository;

    public ResultConsumer(IResultRepository resultRepository)
    {
        _resultsRepository = resultRepository;
    }

    public async Task Consume(ConsumeContext<AsyncAPI.Models.Result> context)
    {
        var model = context.Message;
        var dbResult = new HttpAPI.Models.Result
        {
            submissionId = model.submission,
            receptorId = model.receptor,
            affinity = model.affinity,
            fullOutputPath = model.fullOutputPath
        };
        await _resultsRepository.CreateAsync(dbResult);
    }
}