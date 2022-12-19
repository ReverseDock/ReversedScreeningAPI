using Microsoft.AspNetCore.Mvc;

using HttpAPI.Models.DTO;
using Services;

namespace HttpAPI.Controllers;

[ApiController]
[Route("submissions")]
public class SubmissionController : ControllerBase
{
    private readonly ILogger<SubmissionController> _logger;
    private readonly ISubmissionService _submissionService;
    private readonly IDockingPrepService _dockingPrepService;
    private readonly IReceptorService _receptorService;
    private readonly IFileService _fileService;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;


    public SubmissionController(ILogger<SubmissionController> logger,
                                ISubmissionService submissionService,
                                IDockingPrepService dockingPrepService,
                                IFileService fileService, IReceptorService receptorService,
                                IConfiguration configuration, IMailService mailService)
    {
        _logger = logger;
        _submissionService = submissionService;
        _dockingPrepService = dockingPrepService;
        _fileService = fileService;
        _receptorService = receptorService;
        _configuration = configuration;
        _mailService = mailService;
    }

    [HttpPost]
    public async Task<ActionResult> CreateSubmission(IFormFile ligandFile)
    {
        var userIP = Request.HttpContext.Connection.RemoteIpAddress;
        var submission = await _submissionService.CreateSubmission(ligandFile, userIP!.ToString());
        return Ok(submission.guid);
    }

    [HttpPost]
    [Route("{submissionGuid}/receptors")]
    public async Task<ActionResult> AddReceptors(Guid submissionGuid, IFormFile receptorsFile)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();
        if (submission.status >= Models.SubmissionStatus.Confirmed) return BadRequest("Can't change receptors, submission already confirmed.");

        var file = await _submissionService.AddReceptors(submission, receptorsFile);
        if (file is null) return BadRequest("File could not be parsed.");
    
        submission = await _submissionService.GetSubmission(submissionGuid);
        var receptorsCountCheckResult = await _submissionService.CheckReceptorsCount(submission!);
        var maxReceptors = int.Parse(_configuration.GetSection("Limitations")["MaxReceptorAmount"]);
        if (!receptorsCountCheckResult) return BadRequest($"Too many receptors provided. Maximum is {maxReceptors}.");

        var exhaustiveness = await _submissionService.CalculateAndSetExhaustiveness(submission!);
        var receptorsDTO = await _submissionService.GetReceptorDTOs(submission!);
        var result = new ReceptorStatusDTO { exhaustiveness = exhaustiveness, receptors = receptorsDTO };
        return Ok(result);
    }

    [HttpPost]
    [Route("{submissionGuid}/confirm")]
    public async Task<ActionResult> CreateConfirmation(Guid submissionGuid, [FromForm] string? emailAddress)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();
        if (emailAddress is not null)
        {
            if (!CheckEmailAddress(emailAddress)) return BadRequest("Invalid email address provided");
            submission.emailAddress = emailAddress;
        } 

        await _submissionService.UpdateSubmission(submission);
        // await _mailService.PublishConfirmationMail(submission);
        return Ok(submission.confirmationGuid);
    }

    [HttpPost]
    [Route("{submissionGuid}/confirm/{confirmationGuid}")]
    public async Task<ActionResult> ConfirmSubmission(Guid submissionGuid, Guid confirmationGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();
        if (submission.confirmationGuid != confirmationGuid) return BadRequest("Wrong confirmation Guid provided.");
        if (submission.status != Models.SubmissionStatus.ConfirmationPending) return Conflict();

        await _submissionService.ConfirmSubmission(submission);
        await _dockingPrepService.PrepareForDocking(submission);
        await _mailService.PublishConfirmedMail(submission);
        return Ok();
    }

    [HttpGet]
    [Route("{submissionGuid}")]
    public async Task<ActionResult> GetInfo(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();

        var ligandFile = await _fileService.GetFile(submission.fileId!);
        if (ligandFile is null) return Conflict();
        var receptorList = await _fileService.GetFile(submission.receptorListFileId!);
        if (receptorList is null) return Conflict();
        var submissionTime = submission.createdAt!;

        var submissionInfo = new SubmissionInfoDTO
        {
            ligandFileName = Path.GetFileNameWithoutExtension(ligandFile.path),
            receptorListFilename = Path.GetFileNameWithoutExtension(receptorList.path), 
            submissionTime = submissionTime.Value.ToUniversalTime().ToString("dd.MM.yyyy HH:mm:ss \"UTC\"")
        };
        return Ok(submissionInfo);
    }

    [HttpGet]
    [Route("{submissionGuid}/status")]
    public async Task<ActionResult> GetStatus(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();

        var status = await _submissionService.GetStatus(submission);
        return Ok(status);
    }

    [HttpGet]
    [Route("{submissionGuid}/progress")]
    public async Task<ActionResult> GetProgress(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();

        var progress = await _submissionService.GetProgress(submission);
        return Ok(progress);
    }

    [HttpGet]
    [Route("{submissionGuid}/results")]
    public async Task<ActionResult> GetResults(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();

        var results = await _submissionService.GetResults(submission);
        var dto = new SubmissionResultsDTO
        {
            dockingResults = results
        };
        return Ok(dto);
    }

    [HttpGet]
    [Route("{submissionGuid}/results/{resultGuid}")]
    public async Task<IActionResult> GetResultFile(Guid submissionGuid, Guid resultGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound("Submission not found.");
        var result = await _submissionService.GetResult(submission, resultGuid);
        if (result is null) return NotFound("Result not found.");

        var receptor = await _receptorService.GetReceptor(result.receptorId);
        if (receptor is null) return Conflict();

        var file = await _submissionService.GetResultFile(submission, resultGuid);
        if (file is null) return Conflict();

        var fileStream = await _fileService.GetFileStream(file!.id!);
        if (fileStream is null) return Conflict();

        return File(fileStream, "application/octet-stream", fileDownloadName: Path.GetFileName(file.path));
    }

    [Route("{submissionGuid}/ligand")]
    [HttpGet]
    public async Task<IActionResult> GetFile(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();

        var file = await _fileService.GetFile(submission.fileId!);
        if (file is null) return Conflict();

        var fileStream = await _fileService.GetFileStream(submission.fileId!);
        if (fileStream is null) return Conflict();

        return File(fileStream, "application/octet-stream", fileDownloadName: Path.GetFileName(file.path));
    }

    [Route("{submissionGuid}/receptors")]
    [HttpGet]
    public async Task<IActionResult> GetReceptorsFile(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();

        var file = await _fileService.GetFile(submission.receptorListFileId!);
        if (file is null) return Conflict();

        var fileStream = await _fileService.GetFileStream(submission.receptorListFileId!);
        if (fileStream is null) return Conflict();

        return File(fileStream, "application/octet-stream", fileDownloadName: Path.GetFileName(file.path));
    }

    private bool CheckEmailAddress(string emailAddress)
    {
        // Thank you "imjosh", https://stackoverflow.com/a/16403290
        var trimmedEmail = emailAddress.Trim();

        if (trimmedEmail.EndsWith("."))
        {
            return false;
        }

        try
        {
            var addr = new System.Net.Mail.MailAddress(emailAddress);
            return addr.Address == trimmedEmail;
        } catch
        {
            return false;
        }
    }
}
