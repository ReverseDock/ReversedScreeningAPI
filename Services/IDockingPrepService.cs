using HttpAPI.Models;

namespace Services;

public interface IDockingPrepService
{
    public Task PrepareForDocking(Submission submission, Receptor receptor);
    public Task PrepareForDocking(Submission submission);
}