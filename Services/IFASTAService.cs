using HttpAPI.Models;

namespace Services;

public interface IFASTAService
{
    public Task PublishFASTATask(Submission submission, Receptor receptor);
}