using System.Net.Http.Headers;

using DataAccess.Repositories;

using HttpAPI.Models;

namespace Services;

class UserFileService : IUserFileService
{
    private readonly ILogger<UserFileService> _logger;
    private readonly IUserFileRepository _fileRepository;
    private readonly IConfiguration _configuration;
    
    public UserFileService(ILogger<UserFileService> logger, IUserFileRepository fileRepository,
                           IConfiguration configuarion)
    {
        _logger = logger;
        _fileRepository = fileRepository;
        _configuration = configuarion;
    }

    public async Task<UserFile?> CreateFile(IFormFile formFile)
    {
        try
        {
            var guid = Guid.NewGuid();
            var file = formFile;
            var pathToSave = Path.Combine(_configuration.GetSection("Storage")["UserFiles"], guid.ToString());
            Directory.CreateDirectory(pathToSave);
            
            var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName!.Trim('"');
            var fullPath = Path.Combine(pathToSave, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            var result = await _fileRepository.CreateAsync(new UserFile {
                fullPath = fullPath,
                guid = guid,
                FASTA = ""
            });

            return result;
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

    public async Task<UserFile?> GetFile(string id)
    {
        return await _fileRepository.GetAsync(id);
    }
}