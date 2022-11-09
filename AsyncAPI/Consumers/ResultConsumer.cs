using EasyNetQ;
using EasyNetQ.Consumer;
using Models;

namespace AsyncAPI.Consumers;

public class ResultConsumer
{
    public ResultConsumer()
    {
        Console.WriteLine("Created instance of ResultConsumer");
    }

    public async Task Consume(IMessage<Result> msg, MessageReceivedInfo info)
    {
        await Task.CompletedTask;
        Console.WriteLine($"got message with affinity {msg.Body.affinity}");
    }

    ~ResultConsumer()
    {
        Console.WriteLine("Deleting instance of ResultConsumer");
    }
}