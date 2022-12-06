using Microsoft.AspNetCore.Mvc;

using HttpAPI.Models.DTO;
using Services;

namespace HttpAPI.Controllers;

[ApiController]
[Route("submissions")]
public class SubmissionController : ControllerBase
{
    private readonly ILogger<SubmissionController> _logger;
    private readonly IUserFileService _userFileService;
    private readonly ISubmissionService _submissionService;
    private readonly IDockingPrepService _dockingPrepService;

    public SubmissionController(ILogger<SubmissionController> logger,
                                IUserFileService userFileService,
                                ISubmissionService submissionService,
                                IDockingPrepService dockingPrepService)
    {
        _logger = logger;
        _userFileService = userFileService;
        _submissionService = submissionService;
        _dockingPrepService = dockingPrepService;
    }

    [HttpPost]
    public async Task<ActionResult> CreateSubmission([FromForm] Guid fileGuid, [FromForm] string emailAddress)
    {
        var userIP = Request.HttpContext.Connection.RemoteIpAddress;
        var guid = await _submissionService.CreateSubmission(fileGuid, emailAddress, userIP!.ToString());
        return Ok(guid);
    }

    [HttpPost]
    [Route("{submissionGuid}/confirm")]
    public async Task<ActionResult> ConfirmSubmission(Guid submissionGuid)
    {
        await _submissionService.ConfirmSubmission(submissionGuid);
        var submission = await _submissionService.GetSubmission(submissionGuid);
        var userFile = await _userFileService.GetFile(submission!.fileId!);
        await _dockingPrepService.PrepareForDocking(null, userFile, submission);
        return Ok();
    }

    [HttpGet]
    [Route("{submissionGuid}")]
    public async Task<ActionResult> GetResults(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        var results = await _submissionService.GetResults(submissionGuid);
        var userFile = await _userFileService.GetFile(submission!.fileId!);
        var dto = new SubmissionInfoDTO
        {
            ligandFASTA = userFile!.FASTA,
            dockingResults = results
        };
        return Ok(dto);
    }
}
