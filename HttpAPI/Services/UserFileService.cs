using System.Net.Http.Headers;

using DataAccess.Repositories;

using HttpAPI.Models;

namespace HttpAPI.Services;

class UserFileService : IUserFileService
{
    private readonly ILogger<UserFileService> _logger;
    private readonly IUserFileRepository _fileRepository;
    
    public UserFileService(ILogger<UserFileService> logger, IUserFileRepository fileRepository)
    {
        _logger = logger;
        _fileRepository = fileRepository;
    }

    public async Task<Guid?> CreateFile(IFormFile formFile)
    {
        try
        {
            var guid = Guid.NewGuid();
            var file = formFile;
            var folderName = Path.Combine("UserFiles");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            Directory.CreateDirectory(pathToSave);
            
            var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName!.Trim('"');
            var fullPath = Path.Combine(pathToSave, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            await _fileRepository.CreateAsync(new UserFile {
                fullPath = fullPath,
                guid = guid,
                FASTA = Guid.NewGuid().ToString()
            });

            return guid;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error when creating file: {ex}");
            return null;
        }
    }

    public async Task<List<UserFile>> GetFiles()
    {
        return await _fileRepository.GetAsync();
    }

    public async Task<FileStream?> GetFile(Guid guid)
    {
         var fileObject = await _fileRepository.GetByGuid(guid);

        if (fileObject == null) return null;

        var fileStream = new FileStream(fileObject.fullPath, FileMode.Open);
        
        return fileStream;
    }
}