using HttpAPI.Models;

namespace Services;

public interface IUserFileService
{
    public Task<FileStream?> GetFile(Guid guid);
    public Task<UserFile?> GetFile(string id);
    public Task<UserFile?> CreateFile(IFormFile formFile);
    public Task<List<UserFile>> GetFiles();
    public Task<UserFile?> GetFileByGuid(Guid guid);
}