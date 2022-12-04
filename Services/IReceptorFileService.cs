using HttpAPI.Models;

namespace Services;

public interface IReceptorFileService
{
    public Task<FileStream?> GetFile(string id);
    public Task<ReceptorFile?> CreateFile(IFormFile formFile, int group);
    public Task<List<ReceptorFile>> GetFiles();
}