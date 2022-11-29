using Microsoft.AspNetCore.Mvc;

using DataAccess.Repositories;

using HttpAPI.Models;
using HttpAPI.Services;

namespace HttpAPI.Controllers;

[ApiController]
[Route("admin")]
public class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;
    private readonly IReceptorFileService _receptorFileService;
    private readonly IFASTAService _FASTAService;

    public AdminController(ILogger<AdminController> logger,
                           IReceptorFileService receptorFileService,
                           IFASTAService FASTAService)
    {
        _logger = logger;
        _receptorFileService = receptorFileService;
        _FASTAService = FASTAService;
    }

    [HttpPost]
    [Route("receptors")]
    public async Task<ActionResult> CreateReceptorFile(IFormFile formFile, [FromQuery] int group) 
    {
        if (formFile.Length == 0) return BadRequest();
        var result = await _receptorFileService.CreateFile(formFile, group);
        if (result is null) return BadRequest();
        await _FASTAService.PublishFASTATask(result);
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
