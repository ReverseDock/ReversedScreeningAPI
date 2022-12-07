using HttpAPI.Models;
using HttpAPI.Models.DTO;

namespace Services;

public interface ISubmissionService
{
    public Task<Submission> CreateSubmission(IFormFile ligandFile, string ipAddress);
    public Task ConfirmSubmission(Guid submissionGuid, Guid confirmationGuid);
    public Task CreateDockings(string submissionId);
    public Task<List<DockingResultDTO>> GetResults(Guid submissionGuid);
    public Task<Submission?> GetSubmission(Guid submissionGuid);
    public Task AddReceptors(Guid submissionGuid, IFormFile file);
    public Task<FileDescriptor?> GetResultFile(Guid submissionGuid, Guid resultGuid);
    public Task<DockingResult?> GetResult(Guid submissionGuid, Guid resultGuid);
    public Task<IEnumerable<string>> GetUniProtIdsFromSubmission(string submissionId);
    public Task<List<Submission>> GetSubmissions();
}