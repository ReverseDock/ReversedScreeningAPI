using AsyncAPI.Models;

using MassTransit;

namespace AsyncAPI.Publishers;

public class DockingPrepTaskPublisher : IDockingPrepTaskPublisher, IDisposable
{
    IPublishEndpoint _publishEndpoint;

    public DockingPrepTaskPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishDockingPrepTask(DockingPrepTask dockingPrepTask)
    {
        await _publishEndpoint.Publish<DockingPrepTask>(dockingPrepTask);
    }

    public void Dispose() {}
}