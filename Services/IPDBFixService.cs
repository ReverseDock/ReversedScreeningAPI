using HttpAPI.Models;

namespace Services;

public interface IPDBFixService
{
    public Task PublishPDBFixTask(Submission submission);
}