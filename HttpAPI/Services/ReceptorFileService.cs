using System.Net.Http.Headers;

using DataAccess.Repositories;

using HttpAPI.Models;

namespace HttpAPI.Services;

class ReceptorFileService : IReceptorFileService
{
    private readonly ILogger<ReceptorFileService> _logger;
    private readonly IRepository<ReceptorFile> _fileRepository;
    
    public ReceptorFileService(ILogger<ReceptorFileService> logger, IRepository<ReceptorFile> fileRepository)
    {
        _logger = logger;
        _fileRepository = fileRepository;
    }

    public async Task<bool> CreateFile(IFormFile formFile, int group)
    {
        try
        {
            var file = formFile;
            var folderName = Path.Combine("Receptors");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            Directory.CreateDirectory(pathToSave);
            
            var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName!.Trim('"');
            var fullPath = Path.Combine(pathToSave, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            await _fileRepository.CreateAsync(new ReceptorFile {
                fullPath = fullPath,
                group = group
            });
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error when creating receptor file: {ex}");
            return false;
        }
    }

    public async Task<List<ReceptorFile>> GetFiles()
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