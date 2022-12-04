using HttpAPI.Models;
using HttpAPI.Models.DTO;

namespace Services;

public interface ISubmissionService
{
    public Task<Guid> CreateSubmission(Guid fileId, string emailAddress, string IP);
    public Task ConfirmSubmission(Guid submissionId);
    public Task CreateDockings(string submissionId);
    public Task<List<DockingResultDTO>> GetResults(Guid submissionGuid);
    public Task<Submission?> GetSubmission(Guid submissionGuid);
}