using Microsoft.AspNetCore.Mvc;

using Services;

namespace HttpAPI.Controllers;

[ApiController]
[Route("files")]
public class FileController : ControllerBase
{
    private readonly ILogger<FileController> _logger;
    private readonly IUserFileService _userFileService;
    private readonly ISubmissionService _submissionService;
    private readonly IFASTAService _FASTAService;
    private readonly IPDBFixService _PDBFixService;

    public FileController(ILogger<FileController> logger,
                          IUserFileService userFileService,
                          ISubmissionService submissionService,
                          IFASTAService FASTAService,
                          IPDBFixService PDBFixService)
    {
        _logger = logger;
        _userFileService = userFileService;
        _submissionService = submissionService;
        _FASTAService = FASTAService;
        _PDBFixService = PDBFixService;
    }

    [HttpPost]
    public async Task<ActionResult> PostFile(IFormFile formFile) 
    {
        if (formFile.Length == 0) return BadRequest();
        var result = await _userFileService.CreateFile(formFile);
        if (result is null) return BadRequest();
        await _FASTAService.PublishFASTATask(null, result);
        await _PDBFixService.PublishPDBFixTask(result);
        return Ok(result.guid);
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
