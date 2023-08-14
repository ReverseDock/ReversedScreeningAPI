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
    private readonly IDockingPrepService _dockingPrepService;
    private readonly ISubmissionService _submissionService;

    public AdminController(ILogger<AdminController> logger,
                           IFASTAService FASTAService,
                           IDockingPrepService dockingPrepService,
                           IFileService fileService,
                           ISubmissionService submissionService)
    {
        _logger = logger;
        _FASTAService = FASTAService;
        _dockingPrepService = dockingPrepService;
        _fileService = fileService;
        _submissionService = submissionService;
    }

    [HttpGet]
    [Route("submissions")]
    public async Task<ActionResult> GetSubmissions()
    {
        var result = await _submissionService.GetSubmissions();
        return Ok(result);
    }
}
