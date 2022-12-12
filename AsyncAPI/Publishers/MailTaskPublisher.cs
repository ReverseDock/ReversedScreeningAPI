using AsyncAPI.Models;

using MassTransit;

namespace AsyncAPI.Publishers;

public class MailTaskPublisher : IMailTaskPublisher, IDisposable
{
    IPublishEndpoint _publishEndpoint;

    public MailTaskPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishMailTask(MailTask task)
    {
        await _publishEndpoint.Publish<MailTask>(task);
    }

    public void Dispose() {}
}