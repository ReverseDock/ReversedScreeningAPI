using System.Net.Http.Headers;

using DataAccess.Repositories;

using HttpAPI.Models;

namespace Services;

class FileService : IFileService
{
    private readonly ILogger<FileService> _logger;
    private readonly IFileRepository _fileRepository;
    private readonly IConfiguration _configuration;
    
    public FileService(ILogger<FileService> logger, IFileRepository fileRepository,
                           IConfiguration configuarion)
    {
        _logger = logger;
        _fileRepository = fileRepository;
        _configuration = configuarion;
    }

    public async Task<FileDescriptor> CreateFile(IFormFile formFile, string directory, bool isPublic = false)
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

        var result = await _fileRepository.CreateAsync(new FileDescriptor {
            guid = guid,
            isPublic = isPublic,
            path = fullPath
        });

        return result;
    }

    public async Task<FileDescriptor> CreateFile(string path, string directory, bool isPublic = false)
    {
        var guid = Guid.NewGuid();
        var pathToSave = Path.Combine(_configuration.GetSection("Storage")["Files"], directory, guid.ToString());
        Directory.CreateDirectory(pathToSave);

        var fileName = Path.GetFileName(path);
        var fullPath = Path.Combine(pathToSave, fileName);

        System.IO.File.Move(path, fullPath);

        var result = await _fileRepository.CreateAsync(new FileDescriptor {
            guid = guid,
            isPublic = isPublic,
            path = fullPath
        });

        return result;
    }

    public async Task<List<FileDescriptor>> GetFiles()
    {
        return await _fileRepository.GetAsync();
    }

    public async Task<FileStream?> GetFileStream(Guid guid)
    {
        var fileObject = await _fileRepository.GetByGuid(guid);

        if (fileObject == null) return null;

        var fileStream = new FileStream(fileObject.path, FileMode.Open);
        
        return fileStream;
    }

    public async Task<FileStream?> GetFileStream(string id)
    {
        var fileObject = await _fileRepository.GetAsync(id);

        if (fileObject == null) return null;

        var fileStream = new FileStream(fileObject.path, FileMode.Open);
        
        return fileStream;
    }

    public async Task<FileDescriptor?> GetFile(string id)
    {
        return await _fileRepository.GetAsync(id);
    }

    public async Task<FileDescriptor?> GetFileByGuid(Guid guid)
    {
        return await _fileRepository.GetByGuid(guid);
    }

    public async Task RemoveFile(string id)
    {
        var file = await _fileRepository.GetAsync(id);
        if (file is null)
        {
            _logger.LogWarning($"Trying to delete non-existent file with id {id}");
            return;
        }
        var directory = Path.GetDirectoryName(file.path);
        try
        {
            File.Delete(file.path);
            Directory.Delete(directory!);
        }
        catch (IOException e)
        {
            _logger.LogError($"Error when trying to delete file {file.path} and its directory {directory!}: {e.ToString()}");
            throw e;
        }
        await _fileRepository.RemoveAsync(id);
    }
}