using Microsoft.AspNetCore.Mvc;

using DataAccess.Repositories;

using HttpAPI.Models;
using HttpAPI.Services;

namespace HttpAPI.Controllers;

[ApiController]
[Route("Submissions")]
public class SubmissionController : ControllerBase
{
    private readonly ILogger<DockingPublishController> _logger;
    private readonly IRepository<Submission> _submissionRepository;
    private readonly IUserFileService _userFileService;

    public SubmissionController(ILogger<DockingPublishController> logger,
                                IRepository<Submission> submissionRepository,
                                IUserFileService userFileService)
    {
        _logger = logger;
        _submissionRepository = submissionRepository;
        _userFileService = userFileService;
    }

    [HttpPost(Name = "PostSubmission")]
    public async Task<ActionResult> PostSubmission(IFormFile formFile) 
    {
        if (formFile.Length == 0) return BadRequest();
        var result = await _userFileService.CreateFile(formFile);
        if (!result) return BadRequest();
        return Ok();
    }

    [HttpGet(Name = "GetFiles")]
    public async Task<ActionResult> GetFiles()
    {
        var result = await _userFileService.GetFiles();
        return Ok(result);
    }

    [Route("{fileId}")]
    [HttpGet]
    public async Task<IActionResult> GetFile(string fileId)
    {
        var fileStream = await _userFileService.GetFile(fileId);
        if (fileStream is null) return NotFound();
        return File(fileStream, "application/octet-stream");
    }
}
