using Microsoft.AspNetCore.Mvc;

using DataAccess.Repositories;

using HttpAPI.Models;
using HttpAPI.Services;

namespace HttpAPI.Controllers;

[ApiController]
[Route("submissions")]
public class SubmissionController : ControllerBase
{
    private readonly ILogger<SubmissionController> _logger;
    private readonly IUserFileService _userFileService;
    private readonly ISubmissionService _submissionService;

    public SubmissionController(ILogger<SubmissionController> logger,
                                IUserFileService userFileService,
                                ISubmissionService submissionService)
    {
        _logger = logger;
        _userFileService = userFileService;
        _submissionService = submissionService;
    }

    [HttpPost]
    public async Task<ActionResult> CreateSubmission(Guid fileGuid, string emailAddress)
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
        await _submissionService.CreateDockings(submissionGuid);
        return Ok();
    }

    [HttpGet]
    [Route("{submissionGuid}")]
    public async Task<ActionResult> GetResults(Guid submissionGuid)
    {
        var results = await _submissionService.GetResults(submissionGuid);
        return Ok(results);
    }
}
