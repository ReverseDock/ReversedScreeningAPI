using System.Net.Http.Headers;

using System.Xml;

using DataAccess.Repositories;

using HttpAPI.Models;

using HttpAPI.Models.DTO;

namespace Services;

class AlphaFoldReceptorService : IAlphaFoldReceptorService
{
    private readonly ILogger<AlphaFoldReceptorService> _logger;
    private readonly IAlphaFoldReceptorRepository _receptorRepository;
    private readonly IFileService _fileService;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public AlphaFoldReceptorService(ILogger<AlphaFoldReceptorService> logger, IAlphaFoldReceptorRepository receptorRepository,
                               IConfiguration configuration, IFileService fileService,
                               HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _fileService = fileService;
        _receptorRepository = receptorRepository;
        _httpClient = httpClient;
    }

    public async Task<AlphaFoldReceptor?> CreateReceptor(IFormFile formFile, string UniProtId)
    {
        try
        {
            var file = _fileService.CreateFile(formFile, "receptors", true);

            // var name = await GetReceptorName(UniProtId);

            var alphaFoldReceptor = await _receptorRepository.CreateAsync(new AlphaFoldReceptor {
                FASTA = "",
                UniProtID = UniProtId,
                file = file
            });

            return alphaFoldReceptor;
        }   
        catch (Exception ex)
        {
            _logger.LogError($"Error when creating receptor file: {ex}");
            return null;
        }
    }
    /*
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

    public async Task<Receptor?> GetReceptorForUniProtId(string uniProtId)
    {
        return await _receptorRepository.GetByUniProtIdAsync(uniProtId);
    }

    public async Task<string> GetReceptorName(string uniProtId)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync($"https://rest.uniprot.org/uniprotkb/{uniProtId}.xml");
        } catch (HttpRequestException httpRequestException)
        {
            _logger.LogError($"Could not get name for UniProtId. Status code: {httpRequestException.StatusCode}");
            return "";
        }
        var xmlDocument = new XmlDocument();
        var fileStream = await response.Content.ReadAsStreamAsync();
        xmlDocument.Load(fileStream);
        var recommendName = xmlDocument.GetElementsByTagName("recommendedName")[0];
        var name = recommendName?.ChildNodes[0]?.InnerText;
        var organism = xmlDocument.GetElementsByTagName("organism")[0];
        var organismName = organism?.ChildNodes[0]?.InnerText;
        if (name == null) return "";
        return name + (organismName == null ? "" : $" ({organismName})");
    }

    public async Task UpdateNames()
    {
        var receptorsWithoutNames = await _receptorRepository.GetReceptorsWithoutNames();
        foreach (var x in receptorsWithoutNames)
        {
            var name = await GetReceptorName(x.UniProtID);
            x.name = name;
            await _receptorRepository.UpdateAsync(x.id!, x);
        }
    }

    public async Task<List<ReceptorDTO>> GetReceptorDTOs(IEnumerable<string> uniProtIds)
    {
        var result = new List<ReceptorDTO>(); 
        foreach (var id in uniProtIds)
        {
            var receptor = await _receptorRepository.GetByUniProtIdAsync(id);
            if (receptor is null)
            {
                result.Add(new ReceptorDTO { UniProtId = id, status = "NotFound" });
                continue;
            }
            if (receptor.status == ReceptorFileStatus.TooBig)
            {
                result.Add(new ReceptorDTO { UniProtId = id, name = receptor.name, status = "TooBig" });
                continue;
            }
            if (receptor.status == ReceptorFileStatus.PDBQTError)
            {
                result.Add(new ReceptorDTO { UniProtId = id, name = receptor.name, status = "Error" });
                continue;
            }
            result.Add(new ReceptorDTO { UniProtId = id, name = receptor.name, status = "Okay" });
        }
        return result;
    }
    */
}