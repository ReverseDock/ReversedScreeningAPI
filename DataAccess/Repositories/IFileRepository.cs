using HttpAPI.Models;

namespace DataAccess.Repositories;

public interface IFileRepository : IRepository<FileDescriptor>
{
    public Task<FileDescriptor?> GetByGuid(Guid guid);
}