using HttpAPI.Models;

namespace HttpAPI.Services;

public interface IUserFileService
{
    public Task<FileStream?> GetFile(Guid guid);
    public Task<Guid?> CreateFile(IFormFile formFile);
    public Task<List<UserFile>> GetFiles();
}