using HttpAPI.Models;

namespace DataAccess.Repositories;

public interface IReceptorRepository : IRepository<Receptor>
{
    public Task<List<Receptor>> GetAsync(IEnumerable<string> uniProtIds);
    public Task<Receptor?> GetByUniProtIdAsync(string uniProtId);
}