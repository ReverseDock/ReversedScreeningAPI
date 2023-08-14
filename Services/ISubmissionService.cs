using HttpAPI.Models;
using HttpAPI.Models.DTO;

namespace Services;

public interface ISubmissionService
{
    public Task<Submission> CreateSubmission(IFormFile ligandFile, string ipAddress);
    public Task ConfirmSubmission(Submission submission);
    public Task CreateDockings(Submission submission);
    public Task<int> GetUnfinishedDockingsCount();
    public List<DockingResultDTO> GetResults(Submission submission);
    public Task<Submission?> GetSubmission(string submissionId);
    public Task<Submission?> GetSubmission(Guid submissionGuid);
    public Task AddReceptors(Submission submission, IFormFileCollection files);
    public Task<FileDescriptor?> GetResultFile(Submission submission, Guid resultGuid);
    public Task<List<Submission>> GetSubmissions();
    public Task UpdateSubmission(Submission submission);
    public Task<SubmissionStatus?> GetStatus(Submission submission);
    public float GetProgress(Submission submission);
    public Task<IEnumerable<Receptor>> GetUnprocessedReceptors(Submission submission);
    
}