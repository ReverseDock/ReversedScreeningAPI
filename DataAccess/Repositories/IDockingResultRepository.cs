using HttpAPI.Models;
using HttpAPI.Models.DTO;

namespace DataAccess.Repositories;

public interface IDockingResultRepository : IRepository<DockingResult>
{
    public Task<List<DockingResult>> GetBySubmissionId(string submissionId);

    public Task<List<DockingResultDTO>> GetDTOAsync(string submissionId);
    
    public Task<DockingResult?> GetByGuid(Guid resultGuid);
}