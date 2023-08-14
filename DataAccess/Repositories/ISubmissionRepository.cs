using HttpAPI.Models;

namespace DataAccess.Repositories;

public interface ISubmissionRepository : IRepository<Submission>
{
    public Task<Submission?> GetByGuid(Guid guid);

    public Task UpdateReceptor(Submission submission, Receptor receptor);

    public Task<Receptor?> GetReceptor(Submission submission, Guid guid);

}