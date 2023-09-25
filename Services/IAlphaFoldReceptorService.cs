using HttpAPI.Models;
using HttpAPI.Models.DTO;

namespace Services;

public interface IAlphaFoldReceptorService
{
    /*
    public Task<Receptor?> GetReceptor(string id);
    public Task<Receptor?> GetReceptorForUniProtId(string uniProtId);
    */
    public Task<AlphaFoldReceptor?> CreateReceptor(IFormFile formFile, string UniProtId);
    /*
    public Task<List<Receptor>> GetReceptors();
    public Task<List<Receptor>> GetReceptorsForUniProtIds(IEnumerable<string> uniProtIds);
    public Task<List<ReceptorDTO>> GetReceptorDTOs(IEnumerable<string> uniProtIds);
    public Task UpdateNames();
    */
} 