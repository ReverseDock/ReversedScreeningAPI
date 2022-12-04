using System.Net.Http.Headers;

using DataAccess.Repositories;

using HttpAPI.Models;

namespace Services;

class ReceptorFileService : IReceptorFileService
{
    private readonly ILogger<ReceptorFileService> _logger;
    private readonly IRepository<ReceptorFile> _fileRepository;
    private readonly IConfiguration _configuration;
    
    public ReceptorFileService(ILogger<ReceptorFileService> logger, IRepository<ReceptorFile> fileRepository,
                               IConfiguration configuration)
    {
        _logger = logger;
        _fileRepository = fileRepository;
        _configuration = configuration;
    }

    public async Task<ReceptorFile?> CreateFile(IFormFile formFile, int group)
    {
        try
        {
            var file = formFile;
            var pathToSave = Path.Combine(_configuration.GetSection("Storage")["Receptors"], group.ToString());
            Directory.CreateDirectory(pathToSave);
            
            var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName!.Trim('"');
            var fullPath = Path.Combine(pathToSave, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            var receptorFile = await _fileRepository.CreateAsync(new ReceptorFile {
                fullPath = fullPath,
                group = group,
                FASTA = ""
            });

            return receptorFile;
        }   
        catch (Exception ex)
        {
            _logger.LogError($"Error when creating receptor file: {ex}");
            return null;
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