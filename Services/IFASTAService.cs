using HttpAPI.Models;

namespace Services;

public interface IFASTAService
{
    public Task PublishFASTATask(ReceptorFile? receptor = null, UserFile? userFile = null);
}