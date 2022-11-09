using MassTransit;
using Models;

namespace AsyncAPI.Consumers;

public class ResultConsumer : IConsumer<Result>
{
    public async Task Consume(ConsumeContext<Result> context)
    {
        await Task.CompletedTask;
        Console.WriteLine($"Affinity received: {context.Message.affinity}");
    }
}