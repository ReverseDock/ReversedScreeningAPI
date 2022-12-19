using HttpAPI.Models;
using HttpAPI.Models.DTO;

namespace Services;

public interface ISubmissionService
{
    public Task<Submission> CreateSubmission(IFormFile ligandFile, string ipAddress);
    public Task ConfirmSubmission(Submission submission);
    public Task CreateDockings(Submission submission);
    public Task<List<DockingResultDTO>> GetResults(Submission submission);
    public Task<Submission?> GetSubmission(string submissionId);
    public Task<Submission?> GetSubmission(Guid submissionGuid);
    public Task<FileDescriptor?> AddReceptors(Submission submission, IFormFile file);
    public Task<FileDescriptor?> GetResultFile(Submission submission, Guid resultGuid);
    public Task<DockingResult?> GetResult(Submission submission, Guid resultGuid);
    public Task<IEnumerable<string>> GetUniProtIdsFromSubmission(Submission submission);
    public Task<List<Submission>> GetSubmissions();
    public Task UpdateSubmission(Submission submission);
    public Task<SubmissionStatus?> GetStatus(Submission submission);
    public Task<float> GetProgress(Submission submission);
    public Task<bool> ValidateReceptorsFile(FileDescriptor file);
    public Task<bool> CheckReceptorsCount(Submission submission);
    public Task<int> CalculateAndSetExhaustiveness(Submission submission);
    public Task<IEnumerable<ReceptorDTO>> GetReceptorDTOs(Submission submission);
}