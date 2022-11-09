using Models;

namespace DataAccess.Repositories;

public interface ISubmissionRepository
{
    public Task<List<Submission>> GetAsync();
    public Task<Submission?> GetAsync(string id);
    public Task CreateAsync(Submission submission);
    public Task UpdateAsync(string id, Submission updatedSubmission);
    public Task RemoveAsync(string id);
}