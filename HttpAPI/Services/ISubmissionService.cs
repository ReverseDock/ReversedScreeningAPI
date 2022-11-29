using HttpAPI.Models.DTO;

namespace HttpAPI.Services;

public interface ISubmissionService
{
    public Task<Guid> CreateSubmission(Guid fileId, string emailAddress, string IP);
    public Task ConfirmSubmission(Guid submissionId);
    public Task CreateDockings(Guid submissionId);
    public Task<List<ResultDTO>> GetResults(Guid submissionGuid);
}