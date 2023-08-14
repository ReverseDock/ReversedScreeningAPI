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
    private readonly IFileService _fileService;
    private readonly IFASTAService _fastaService;
    private readonly IConfiguration _configuration;
    private readonly IMailService _mailService;

    public SubmissionController(ILogger<SubmissionController> logger,
                                ISubmissionService submissionService,
                                IDockingPrepService dockingPrepService,
                                IFileService fileService, IFASTAService fastaService,
                                IConfiguration configuration, IMailService mailService)
    {
        _logger = logger;
        _submissionService = submissionService;
        _dockingPrepService = dockingPrepService;
        _fileService = fileService;
        _fastaService = fastaService;
        _configuration = configuration;
        _mailService = mailService;
    }

    [HttpGet]
    [Route("status")]
    public async Task<ActionResult> GetQueueStatus()
    {
        return Ok(await _submissionService.GetUnfinishedDockingsCount());
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
    public async Task<ActionResult> AddReceptors(Guid submissionGuid, IFormFileCollection receptorsFiles)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);

        if (submission is null) return NotFound();
        if (submission.status >= Models.SubmissionStatus.Confirmed) return BadRequest("Can't change receptors, submission already confirmed.");

        var maxReceptors = int.Parse(_configuration.GetSection("Limitations")["MaxReceptorAmount"]);
        if (receptorsFiles.Count > maxReceptors) return BadRequest($"Only {maxReceptors} PDB files allowed.");
        if (receptorsFiles.Count == 0) return BadRequest("No PDB files provided.");

        await _submissionService.AddReceptors(submission, receptorsFiles);

        return Ok();
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

        foreach (var receptor in submission.receptors)
        {
            await _fastaService.PublishFASTATask(submission, receptor);
        }

        await _mailService.PublishConfirmedMail(submission);

        return Ok();
    }

    [HttpGet]
    [Route("{submissionGuid}")]
    public async Task<ActionResult> GetInfo(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);

        if (submission is null) return NotFound();

        var submissionInfo = new SubmissionInfoDTO
        {
            ligandFileName = Path.GetFileNameWithoutExtension(submission.ligand.path),
            submissionTime = submission.createdAt!.Value.ToUniversalTime().ToString("dd.MM.yyyy HH:mm:ss \"UTC\"")
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

        var progress = _submissionService.GetProgress(submission);

        return Ok(progress);
    }

    [HttpGet]
    [Route("{submissionGuid}/results")]
    public async Task<ActionResult> GetResults(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);

        if (submission is null) return NotFound();

        var results = _submissionService.GetResults(submission);
        var dto = new SubmissionResultsDTO
        {
            dockingResults = results
        };
        
        return Ok(dto);
    }

    [HttpGet]
    [Route("{submissionGuid}/receptors/{resultGuid}/modes")]
    public async Task<IActionResult> GetBindingModes(Guid submissionGuid, Guid resultGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound("Submission not found.");
        var result = submission.receptors.FirstOrDefault(r => r.guid == resultGuid);
        if (result is null) return NotFound("Result not found.");

        var fileStream = _fileService.GetFileStream(result.outputFile!);
        if (fileStream is null) return Conflict();

        return File(fileStream, "application/octet-stream", fileDownloadName: Path.GetFileName(result.outputFile!.path));
    }

    [HttpGet]
    [Route("{submissionGuid}/receptors/{resultGuid}/pdbqt")]
    public async Task<IActionResult> GetReceptorFilePDBQT(Guid submissionGuid, Guid resultGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound("Submission not found.");
        var result = submission.receptors.FirstOrDefault(r => r.guid == resultGuid);
        if (result is null) return NotFound("Result not found.");

        var fileStream = _fileService.GetFileStream(result.pdbqtFile!);
        if (fileStream is null) return Conflict();

        return File(fileStream, "application/octet-stream", fileDownloadName: Path.GetFileName(result.pdbqtFile!.path));
    }

    [HttpGet]
    [Route("{submissionGuid}/receptors/{resultGuid}/pdb")]
    public async Task<IActionResult> GetReceptorFilePDB(Guid submissionGuid, Guid resultGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound("Submission not found.");
        var result = submission.receptors.FirstOrDefault(r => r.guid == resultGuid);
        if (result is null) return NotFound("Result not found.");

        var fileStream = _fileService.GetFileStream(result.file!);
        if (fileStream is null) return Conflict();

        return File(fileStream, "application/octet-stream", fileDownloadName: Path.GetFileName(result.file!.path));
    }

    [Route("{submissionGuid}/ligand")]
    [HttpGet]
    public async Task<IActionResult> GetFile(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();

        var fileStream = _fileService.GetFileStream(submission.ligand!);
        if (fileStream is null) return Conflict();

        return File(fileStream, "application/octet-stream", fileDownloadName: Path.GetFileName(submission.ligand!.path));
    }

    private static bool CheckEmailAddress(string emailAddress)
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
