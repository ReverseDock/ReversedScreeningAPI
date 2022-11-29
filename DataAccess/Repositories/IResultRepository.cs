using HttpAPI.Models;
using HttpAPI.Models.DTO;

namespace DataAccess.Repositories;

public interface IResultRepository : IRepository<Result>
{
    public Task<List<Result>> GetBySubmissionId(string submissionId);

    public Task<List<ResultDTO>> GetDTOAsync(string submissionId);
}