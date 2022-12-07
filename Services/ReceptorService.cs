using System.Net.Http.Headers;

using DataAccess.Repositories;

using HttpAPI.Models;

using HttpAPI.Models.DTO;

namespace Services;

class ReceptorService : IReceptorService
{
    private readonly ILogger<ReceptorService> _logger;
    private readonly IReceptorRepository _receptorRepository;
    private readonly IFileService _fileService;
    private readonly IConfiguration _configuration;
    
    public ReceptorService(ILogger<ReceptorService> logger, IReceptorRepository receptorRepository,
                               IConfiguration configuration, IFileService fileService)
    {
        _logger = logger;
        _configuration = configuration;
        _fileService = fileService;
        _receptorRepository = receptorRepository;
    }

    public async Task<Receptor?> CreateReceptor(IFormFile formFile, string UniProtId)
    {
        try
        {
            var file = await _fileService.CreateFile(formFile, "receptors");

            var receptorFile = await _receptorRepository.CreateAsync(new Receptor {
                fileId = file!.id,
                FASTA = "",
                UniProtID = UniProtId
            });

            return receptorFile;
        }   
        catch (Exception ex)
        {
            _logger.LogError($"Error when creating receptor file: {ex}");
            return null;
        }
    }

    public async Task<Receptor?> GetReceptor(string id)
    {
        return await _receptorRepository.GetAsync(id);
    }

    public async Task<List<Receptor>> GetReceptors()
    {
        return await _receptorRepository.GetAsync();
    }

    public async Task<List<Receptor>> GetReceptorsForUniProtIds(IEnumerable<string> uniProtIds)
    {
        return await _receptorRepository.GetAsync(uniProtIds);
    }

    public async Task<List<ReceptorStatusDTO>> GetReceptorStatusDTOs(IEnumerable<string> uniProtIds)
    {
        var result = new List<ReceptorStatusDTO>(); 
        foreach (var id in uniProtIds)
        {
            var receptor = await _receptorRepository.GetByUniProtIdAsync(id);
            if (receptor is null)
            {
                result.Add(new ReceptorStatusDTO { UniProtId = id, status = "NotFound" });
                continue;
            }
            if (receptor.status == ReceptorFileStatus.TooBig)
            {
                result.Add(new ReceptorStatusDTO { UniProtId = id, status = "TooBig" });
                continue;
            }
            if (receptor.status == ReceptorFileStatus.PDBQTError)
            {
                result.Add(new ReceptorStatusDTO { UniProtId = id, status = "Error" });
                continue;
            }
            result.Add(new ReceptorStatusDTO { UniProtId = id, status = "Okay" });
        }
        return result;
    }
}