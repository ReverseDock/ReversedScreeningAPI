using HttpAPI.Models;

namespace HttpAPI.Services;

public interface IFASTAService
{
    public Task PublishFASTATask(ReceptorFile? receptor = null, UserFile? userFile = null);
}