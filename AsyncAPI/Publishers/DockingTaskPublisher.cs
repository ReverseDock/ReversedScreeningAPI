using AsyncAPI.Models;

using MassTransit;

namespace AsyncAPI.Publishers;

public class DockingTaskPublisher : IDockingTaskPublisher, IDisposable
{
    IPublishEndpoint _publishEndpoint;

    public DockingTaskPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishDockingTask(DockingTask docking)
    {
        await _publishEndpoint.Publish<DockingTask>(docking);
    }

    public void Dispose() {}
}