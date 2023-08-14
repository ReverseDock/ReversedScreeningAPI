using HttpAPI.Models;

namespace Services;

public interface IFileService
{
    public FileStream? GetFileStream(FileDescriptor fileDescriptor);
    public FileDescriptor CreateFile(IFormFile formFile, string directory, bool isPublic = false);
    public FileDescriptor CreateFile(string path, string directory, bool isPublic = false);
    public void RemoveFile(FileDescriptor fileDescriptor);
}