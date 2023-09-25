using HttpAPI.Models;

namespace DataAccess.Repositories;

public interface IAlphaFoldReceptorRepository : IRepository<AlphaFoldReceptor>
{
    public Task<AlphaFoldReceptor?> GetByUnitProtID(string uniProtId);
}