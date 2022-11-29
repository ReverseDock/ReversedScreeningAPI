using AsyncAPI.Models;

namespace AsyncAPI.Publishers;

public interface IFASTATaskPublisher
{
    public Task PublishFASTATask(FASTATask task);
}