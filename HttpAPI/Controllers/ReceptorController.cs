using Microsoft.AspNetCore.Mvc;

using Services;

namespace HttpAPI.Controllers;

[ApiController]
[Route("receptors")]
public class ReceptorController : ControllerBase
{
    private readonly ILogger<ReceptorController> _logger;
    private readonly IReceptorService _receptorService;
    private readonly IFileService _fileService;

    public ReceptorController(ILogger<ReceptorController> logger,
                              IReceptorService receptorService,
                              IFileService fileService)
    {
        _logger = logger;
        _receptorService = receptorService;
        _fileService = fileService;
    }

    [HttpGet]
    [Route("{uniProtId}/pdbqt")]
    public async Task<ActionResult> GetReceptorPDBQTFile(string uniProtId)
    {
        var result = await _receptorService.GetReceptorForUniProtId(uniProtId);
        if (result is null) return NotFound();
        var file = await _fileService.GetFile(result.pdbqtFileId!);
        var fs = await _fileService.GetFileStream(result.pdbqtFileId!);
        return File(fs!, "applicaton/octet-stream", fileDownloadName: Path.GetFileName(file!.path));
    }
}