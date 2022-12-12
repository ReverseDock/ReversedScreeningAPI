using AsyncAPI.Models;

namespace AsyncAPI.Publishers;

public interface IMailTaskPublisher
{
    public Task PublishMailTask(MailTask mailTask);
}