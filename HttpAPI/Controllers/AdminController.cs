using Microsoft.AspNetCore.Mvc;

using Services;

namespace HttpAPI.Controllers;

[ApiController]
[Route("admin")]
public class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;
    private readonly IReceptorFileService _receptorFileService;
    private readonly IFASTAService _FASTAService;
    private readonly IDockingPrepService _dockingPrepService;

    public AdminController(ILogger<AdminController> logger,
                           IReceptorFileService receptorFileService,
                           IFASTAService FASTAService,
                           IDockingPrepService dockingPrepService)
    {
        _logger = logger;
        _receptorFileService = receptorFileService;
        _FASTAService = FASTAService;
        _dockingPrepService = dockingPrepService;
    }

    [HttpPost]
    [Route("receptors")]
    public async Task<ActionResult> CreateReceptorFile(IFormFile formFile, [FromQuery] int group) 
    {
        if (formFile.Length == 0) return BadRequest();
        var result = await _receptorFileService.CreateFile(formFile, group);
        if (result is null) return BadRequest();
        await _FASTAService.PublishFASTATask(result);
        await _dockingPrepService.PrepareForDocking(result);
        return Ok();
    }

    [HttpGet]
    [Route("receptors")]
    public async Task<ActionResult> GetFiles()
    {
        var result = await _receptorFileService.GetFiles();
        return Ok(result);
    }

}
