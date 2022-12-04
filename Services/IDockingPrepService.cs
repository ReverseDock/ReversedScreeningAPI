using HttpAPI.Models;

namespace Services;

public interface IDockingPrepService
{
    public Task PrepareForDocking(ReceptorFile? receptor = null, UserFile? userFile = null, Submission? submission = null);
}