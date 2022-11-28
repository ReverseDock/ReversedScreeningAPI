using Microsoft.AspNetCore.Mvc;

using DataAccess.Repositories;

using HttpAPI.Models;
using HttpAPI.Services;

namespace HttpAPI.Controllers;

[ApiController]
[Route("files")]
public class FileController : ControllerBase
{
    private readonly ILogger<FileController> _logger;
    private readonly IUserFileService _userFileService;
    private readonly ISubmissionService _submissionService;

    public FileController(ILogger<FileController> logger,
                          IUserFileService userFileService,
                          ISubmissionService submissionService)
    {
        _logger = logger;
        _userFileService = userFileService;
        _submissionService = submissionService;
    }

    [HttpPost]
    public async Task<ActionResult> PostFile(IFormFile formFile) 
    {
        if (formFile.Length == 0) return BadRequest();
        var result = await _userFileService.CreateFile(formFile);
        if (result is null) return BadRequest();
        return Ok(result);
    }

    [Route("{fileGuid}")]
    [HttpGet]
    public async Task<IActionResult> GetFile(Guid fileGuid)
    {
        var fileStream = await _userFileService.GetFile(fileGuid);
        if (fileStream is null) return NotFound();
        return File(fileStream, "application/octet-stream");
    }
}
