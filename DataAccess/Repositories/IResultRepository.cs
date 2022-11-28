using HttpAPI.Models;

namespace DataAccess.Repositories;

public interface IResultRepository : IRepository<Result>
{
    public Task<List<Result>> GetBySubmissionId(string submissionId);
}