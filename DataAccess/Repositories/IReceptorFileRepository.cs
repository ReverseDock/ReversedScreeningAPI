using HttpAPI.Models;

namespace DataAccess.Repositories;

public interface IReceptorFileRepository : IRepository<ReceptorFile>
{
    public Task<List<ReceptorFile>> GetAsync(IEnumerable<string> uniProtIds);
}