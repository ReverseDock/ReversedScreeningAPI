using HttpAPI.Models;

namespace HttpAPI.Services;

public interface IReceptorFileService
{
    public Task<FileStream?> GetFile(string id);
    public Task<bool> CreateFile(IFormFile formFile, int group);
    public Task<List<ReceptorFile>> GetFiles();
}