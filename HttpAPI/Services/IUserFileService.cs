using HttpAPI.Models;

namespace HttpAPI.Services;

public interface IUserFileService
{
    public Task<FileStream?> GetFile(string id);
    public Task<bool> CreateFile(IFormFile formFile);
    public Task<List<UserFile>> GetFiles();
}