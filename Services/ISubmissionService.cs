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
    public Task<string> CreateDirectory(Guid submissionGuid);
    public Task CreateReceptorListFile(Guid submissionGuid, string directory, IFormFile file);
    public Task<FileStream?> GetResultFile(Guid submissionGuid, Guid resultGuid);
    public Task<DockingResult?> GetResult(Guid submissionGuid, Guid resultGuid);
}