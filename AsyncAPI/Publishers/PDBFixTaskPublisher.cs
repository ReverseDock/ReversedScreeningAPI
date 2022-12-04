using AsyncAPI.Models;

using MassTransit;

namespace AsyncAPI.Publishers;

public class PDBFixTaskPublisher : IPDBFixTaskPublisher, IDisposable
{
    IPublishEndpoint _publishEndpoint;

    public PDBFixTaskPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishPDBFixTask(PDBFixTask task)
    {
        await _publishEndpoint.Publish<PDBFixTask>(task);
    }

    public void Dispose() {}
}