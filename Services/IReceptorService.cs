using HttpAPI.Models;
using HttpAPI.Models.DTO;

namespace Services;

public interface IReceptorService
{
    public Task<Receptor?> GetReceptor(string id);
    public Task<Receptor?> GetReceptorForUniProtId(string uniProtId);
    public Task<Receptor?> CreateReceptor(IFormFile formFile, string UniProtId);
    public Task<List<Receptor>> GetReceptors();
    public Task<List<Receptor>> GetReceptorsForUniProtIds(IEnumerable<string> uniProtIds);
    public Task<List<ReceptorStatusDTO>> GetReceptorStatusDTOs(IEnumerable<string> uniProtIds);
}