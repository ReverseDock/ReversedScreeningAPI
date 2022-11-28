using HttpAPI.Models;

namespace DataAccess.Repositories;

public interface ISubmissionRepository : IRepository<Submission>
{
    public Task<Submission?> GetByGuid(Guid guid);
}