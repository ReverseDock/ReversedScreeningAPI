using AsyncAPI.Models;

using MassTransit;

namespace AsyncAPI.Publishers;

public class FASTATaskPublisher : IFASTATaskPublisher, IDisposable
{
    IPublishEndpoint _publishEndpoint;

    public FASTATaskPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishFASTATask(FASTATask FASTA)
    {
        await _publishEndpoint.Publish<FASTATask>(FASTA);
    }

    public void Dispose() {}
}