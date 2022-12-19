using HttpAPI.Models;

namespace Services;

public interface IFASTAService
{
    public Task PublishFASTATask(Receptor receptor);
}