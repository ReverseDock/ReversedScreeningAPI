using MassTransit;
using Models;

namespace AsyncAPI.Publishers;

public class DockingPublisher : IDockingPublisher
{
    IPublishEndpoint _publishEndpoint;

    public DockingPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishDocking(Docking docking)
    {
        await _publishEndpoint.Publish<Docking>(docking);
    }

}