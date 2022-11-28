using HttpAPI.Models;

namespace DataAccess.Repositories;

public interface IUserFileRepository : IRepository<UserFile>
{
    public Task<UserFile?> GetByGuid(Guid guid);
}