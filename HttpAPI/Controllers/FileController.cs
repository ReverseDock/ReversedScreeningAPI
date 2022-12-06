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
        var userFile = await _userFileService.GetFileByGuid(fileGuid);
        if (userFile is null) return NotFound();
        var fileStream = await _userFileService.GetFile(fileGuid);
        if (fileStream is null) return NotFound();
        return File(fileStream, "application/octet-stream", fileDownloadName: Path.GetFileName(userFile.fullPath));
    }

    [Route("{fileGuid}/fix/file")]
    [HttpGet]
    public async Task<IActionResult> GetFixedFile(Guid fileGuid)
    {
        var userFile = await _userFileService.GetFileByGuid(fileGuid);
        if (userFile is null) return NotFound();
        var fileStream = await _PDBFixService.GetFixedFile(userFile);
        if (fileStream is null) return NotFound();
        return File(fileStream, "application/octet-stream", fileDownloadName: Path.GetFileName(userFile.fullFixedPath));
    }

    [Route("{fileGuid}/fix/results")]
    [HttpGet]
    public async Task<IActionResult> GetFixedResults(Guid fileGuid)
    {
        var userFile = await _userFileService.GetFileByGuid(fileGuid);
        if (userFile is null) return NotFound();
        return Ok(userFile.fixedJSONResult);
    }

    [Route("{fileGuid}/fix/status")]
    [HttpGet]
    public async Task<IActionResult> GetFixStatus(Guid fileGuid)
    {
        var userFile = await _userFileService.GetFileByGuid(fileGuid);
        if (userFile is null) return NotFound();
        var status = await _PDBFixService.CheckPDBFixStatus(userFile);
        if (status) return Ok();
        return Conflict();
    }
}
