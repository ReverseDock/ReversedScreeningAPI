using HttpAPI.Models;

namespace HttpAPI.Services;

public interface ISubmissionService
{
    public Task<Guid> CreateSubmission(Guid fileId, string emailAddress, string IP);
    public Task ConfirmSubmission(Guid submissionId);
    public Task CreateDockings(Guid submissionId);
    public Task<List<Models.Result>> GetResults(Guid submissionGuid);
}