using System.Net.Http.Headers;

using DataAccess.Repositories;

using HttpAPI.Models;

namespace Services;

class FileService : IFileService
{
    private readonly ILogger<FileService> _logger;
    private readonly IConfiguration _configuration;
    
    public FileService(ILogger<FileService> logger,
                           IConfiguration configuarion)
    {
        _logger = logger;
        _configuration = configuarion;
    }

    public FileDescriptor CreateFile(IFormFile formFile, string directory, bool isPublic = false)
    {
        var guid = Guid.NewGuid();
        var file = formFile;
        var pathToSave = Path.Combine(_configuration.GetSection("Storage")["Files"], directory, guid.ToString());
        Directory.CreateDirectory(pathToSave);
        
        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName!.Trim('"');
        var fullPath = Path.Combine(pathToSave, fileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        return new FileDescriptor {
            guid = guid,
            isPublic = isPublic,
            path = fullPath
        };
    }

    public FileDescriptor CreateFile(string path, string directory, bool isPublic = false)
    {
        var guid = Guid.NewGuid();
        var pathToSave = Path.Combine(_configuration.GetSection("Storage")["Files"], directory, guid.ToString());
        Directory.CreateDirectory(pathToSave);

        var fileName = Path.GetFileName(path);
        var fullPath = Path.Combine(pathToSave, fileName);

        System.IO.File.Move(path, fullPath);

        return new FileDescriptor {
            guid = guid,
            isPublic = isPublic,
            path = fullPath
        };
    }

    public FileStream? GetFileStream(FileDescriptor fileDescriptor)
    {
        var fileStream = new FileStream(fileDescriptor.path, FileMode.Open);
        
        return fileStream;
    }

    public void RemoveFile(FileDescriptor fileDescriptor)
    {
        var directory = Path.GetDirectoryName(fileDescriptor.path);
        try
        {
            File.Delete(fileDescriptor.path);
            Directory.Delete(directory!);
        }
        catch (IOException e)
        {
            _logger.LogError($"Error when trying to delete file {fileDescriptor.path} and its directory {directory!}: {e.ToString()}");
            throw e;
        }
    }
}