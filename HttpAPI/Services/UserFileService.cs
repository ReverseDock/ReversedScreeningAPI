using System.Net.Http.Headers;

using DataAccess.Repositories;

using HttpAPI.Models;

namespace HttpAPI.Services;

class UserFileService : IUserFileService
{
    private readonly ILogger<UserFileService> _logger;
    private readonly IRepository<UserFile> _fileRepository;
    
    public UserFileService(ILogger<UserFileService> logger, IRepository<UserFile> fileRepository)
    {
        _logger = logger;
        _fileRepository = fileRepository;
    }

    public async Task<bool> CreateFile(IFormFile formFile)
    {
        try
        {
            var file = formFile;
            var folderName = Path.Combine("Uploads");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            Directory.CreateDirectory(pathToSave);
            
            var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName!.Trim('"');
            var fullPath = Path.Combine(pathToSave, fileName);
            var dbPath = Path.Combine(folderName, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            await _fileRepository.CreateAsync(new UserFile {
                fullPath = dbPath
            });
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error when creating file: {ex}");
            return false;
        }
    }

    public async Task<List<UserFile>> GetFiles()
    {
        return await _fileRepository.GetAsync();
    }

    public async Task<FileStream?> GetFile(string id)
    {
         var fileObject = await _fileRepository.GetAsync(id);

        if (fileObject == null) return null;

        var fileStream = new FileStream(fileObject.fullPath, FileMode.Open);
        
        return fileStream;
    }
}