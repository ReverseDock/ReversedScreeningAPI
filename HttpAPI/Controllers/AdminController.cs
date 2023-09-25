using Microsoft.AspNetCore.Mvc;

using Services;

namespace HttpAPI.Controllers;

[ApiController]
[Route("admin")]
[Host("localhost")]
public class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;
    private readonly IFASTAService _FASTAService;
    private readonly IFileService _fileService;
    private readonly IAlphaFoldReceptorService _alphaFoldReceptorService;
    private readonly IDockingPrepService _dockingPrepService;
    private readonly ISubmissionService _submissionService;

    public AdminController(ILogger<AdminController> logger,
                           IFASTAService FASTAService,
                           IDockingPrepService dockingPrepService,
                           IAlphaFoldReceptorService alphaFoldReceptorService,
                           IFileService fileService,
                           ISubmissionService submissionService)
    {
        _logger = logger;
        _FASTAService = FASTAService;
        _dockingPrepService = dockingPrepService;
        _alphaFoldReceptorService = alphaFoldReceptorService;
        _fileService = fileService;
        _submissionService = submissionService;
    }

    [HttpPost]
    [Route("receptors")]
    public async Task<ActionResult> CreateReceptorFile(IFormFile formFile, [FromForm] string UniProtId)
    {
        if (formFile.Length == 0) return BadRequest();
        var receptor = await _alphaFoldReceptorService.CreateReceptor(formFile, UniProtId);
        if (receptor is null) return BadRequest();
        await _FASTAService.PublishFASTATask(receptor);
        return Ok();
    }

/*
    [HttpGet]
    [Route("receptors")]
    public async Task<ActionResult> GetReceptors()
    {
        var result = await _receptorService.GetReceptors();
        return Ok(result);
    }
*/

    [HttpGet]
    [Route("submissions")]
    public async Task<ActionResult> GetSubmissions()
    {
        var result = await _submissionService.GetSubmissions();
        return Ok(result);
    }
}
