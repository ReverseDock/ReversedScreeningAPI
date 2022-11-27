using AsyncAPI.Models;

using MassTransit;

namespace AsyncAPI.Publishers;

public class DockingPublisher : IDockingPublisher, IDisposable
{
    IPublishEndpoint _publishEndpoint;

    public DockingPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
        Console.WriteLine("Created docking publisher");
    }

    public async Task PublishDocking(Docking docking)
    {
        await _publishEndpoint.Publish<Docking>(docking);
    }

    public void Dispose() {}
}