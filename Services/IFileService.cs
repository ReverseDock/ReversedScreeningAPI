using HttpAPI.Models;

namespace Services;

public interface IFileService
{
    public Task<FileStream?> GetFileStream(Guid guid);
    public Task<FileStream?> GetFileStream(string id);
    public Task<FileDescriptor?> GetFile(string id);
    public Task<FileDescriptor?> CreateFile(IFormFile formFile, string directory = "", bool isPublic = false);
    public Task<FileDescriptor?> CreateFile(string path, bool isPublic = false);
    public Task<List<FileDescriptor>> GetFiles();
    public Task<FileDescriptor?> GetFileByGuid(Guid guid);
    public Task RemoveFile(string id);
}