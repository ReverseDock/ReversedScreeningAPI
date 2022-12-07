using HttpAPI.Models;

namespace Services;

public interface IDockingPrepService
{
    public Task PrepareForDocking(Receptor receptor);
    public Task PrepareForDocking(Submission submission);
}