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
    private readonly IPDBFixService _pdbFixService;
    private readonly IDockingPrepService _dockingPrepService;
    private readonly IReceptorService _receptorService;
    private readonly IFASTAService _fastaService;
    private readonly IFileService _fileService;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;


    public SubmissionController(ILogger<SubmissionController> logger,
                                ISubmissionService submissionService,
                                IDockingPrepService dockingPrepService,
                                IFileService fileService, IPDBFixService pdbFixService,
                                IFASTAService fastaService, IReceptorService receptorService,
                                IConfiguration configuration, IMailService mailService)
    {
        _logger = logger;
        _submissionService = submissionService;
        _dockingPrepService = dockingPrepService;
        _fileService = fileService;
        _pdbFixService = pdbFixService;
        _fastaService = fastaService;
        _receptorService = receptorService;
        _configuration = configuration;
        _mailService = mailService;
    }

    [HttpPost]
    public async Task<ActionResult> CreateSubmission(IFormFile ligandFile)
    {
        var userIP = Request.HttpContext.Connection.RemoteIpAddress;
        var submission = await _submissionService.CreateSubmission(ligandFile, userIP!.ToString());
        // await _pdbFixService.PublishPDBFixTask(submission);
        // await _fastaService.PublishFASTATask(submission);
        return Ok(submission.guid);
    }

    [HttpPost]
    [Route("{submissionGuid}/receptors")]
    public async Task<ActionResult> AddReceptors(Guid submissionGuid, IFormFile receptorsFile)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();
        if (submission.status >= Models.SubmissionStatus.Confirmed) return BadRequest("Can't change receptors, submission already confirmed.");
        await _submissionService.AddReceptors(submissionGuid, receptorsFile);
        // Add validation
        var uniProtIds = await _submissionService.GetUniProtIdsFromSubmission(submission!.id!);
        var receptorsDTO = await _receptorService.GetReceptorStatusDTOs(uniProtIds);
        var maxReceptors = int.Parse(_configuration.GetSection("Limitations")["MaxReceptorAmount"]);
        var maxExhaustiveness = int.Parse(_configuration.GetSection("Limitations")["MaxExhaustiveness"]);
        var okayReceptors = receptorsDTO.Where(x => x.status == "Okay").Count();
        if (okayReceptors > maxReceptors)
        {
            submission!.receptorListFileId = null;
            await _submissionService.UpdateSubmission(submission);
            return BadRequest($"Too many UniProtId's provided. Limit is {maxReceptors}.");
        }
        var exhaustiveness = (int) Math.Ceiling((((float) (maxReceptors - okayReceptors + 1)) / (float) (maxReceptors)) * maxExhaustiveness);
        var submissionNew = await _submissionService.GetSubmission(submissionGuid);
        submissionNew!.exhaustiveness = exhaustiveness;
        await _submissionService.UpdateSubmission(submissionNew); 
        return Ok(receptorsDTO);
    }

    [HttpPost]
    [Route("{submissionGuid}/confirm")]
    public async Task<ActionResult> CreateConfirmation(Guid submissionGuid, [FromForm] string emailAddress)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();
        submission.emailAddress = emailAddress;
        await _submissionService.UpdateSubmission(submission);
        await _mailService.PublishConfirmationMail(submission);
        return Ok();
    }

    [HttpPost]
    [Route("{submissionGuid}/confirm/{confirmationGuid}")]
    public async Task<ActionResult> ConfirmSubmission(Guid submissionGuid, Guid confirmationGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();
        await _submissionService.ConfirmSubmission(submissionGuid, confirmationGuid);
        await _dockingPrepService.PrepareForDocking(submission!);
        await _mailService.PublishConfirmedMail(submission!);
        return Ok();
    }

    [HttpGet]
    [Route("{submissionGuid}")]
    public async Task<ActionResult> GetInfo(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();

        var ligandFile = await _fileService.GetFile(submission.fileId!);
        var receptorList = await _fileService.GetFile(submission.receptorListFileId!);
        var submissionTime = submission.createdAt!;

        var submissionInfo = new SubmissionInfoDTO
        {
            ligandFileName = Path.GetFileNameWithoutExtension(ligandFile!.path),
            receptorListFilename = Path.GetFileNameWithoutExtension(receptorList!.path), 
            submissionTime = submissionTime.Value.ToString("dd.MM.yyyy HH:mm:ss")
        };
        return Ok(submissionInfo);
    }

    [HttpGet]
    [Route("{submissionGuid}/status")]
    public async Task<ActionResult> GetStatus(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();
        var status = await _submissionService.GetStatus(submissionGuid);
        return Ok(status);
    }

    [HttpGet]
    [Route("{submissionGuid}/progress")]
    public async Task<ActionResult> GetProgress(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();
        var progress = await _submissionService.GetProgress(submissionGuid);
        return Ok(progress);
    }

    [HttpGet]
    [Route("{submissionGuid}/results")]
    public async Task<ActionResult> GetResults(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        var results = await _submissionService.GetResults(submissionGuid);
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
        var result = await _submissionService.GetResult(submissionGuid, resultGuid);
        var receptor = await _receptorService.GetReceptor(result!.receptorId);
        var file = await _submissionService.GetResultFile(submissionGuid, resultGuid);
        var fileStream = await _fileService.GetFileStream(file!.id!);
        if (fileStream is null) return NotFound();
        return File(fileStream, "application/octet-stream", fileDownloadName: Path.GetFileName(file.path));
    }

    /*
    [Route("{submissionGuid}/ligand/fixed")]
    [HttpGet]
    public async Task<IActionResult> GetFixedFile(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();
        if (submission.fixedFileId is null) return NotFound();
        var file = await _fileService.GetFile(submission.fixedFileId);
        if (file is null) return NotFound();
        var fileStream = await _fileService.GetFileStream(submission.fixedFileId);
        if (fileStream is null) return NotFound();
        return File(fileStream, "application/octet-stream", fileDownloadName: Path.GetFileName(file.path));
    }

    [Route("{submissionGuid}/ligand/fixed/results")]
    [HttpGet]
    public async Task<IActionResult> GetFixedResults(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();
        return Ok(submission.fixedJSONResult);
    }

    [Route("{submissionGuid}/ligand/fixed/status")]
    [HttpGet]
    public async Task<IActionResult> GetFixStatus(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();
        if (submission.fixedFileId != null) return Ok();
        return Conflict();
    }
    */

    [Route("{submissionGuid}/ligand")]
    [HttpGet]
    public async Task<IActionResult> GetFile(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();
        var file = await _fileService.GetFile(submission.fileId!);
        if (file is null) return NotFound();
        var fileStream = await _fileService.GetFileStream(submission.fileId!);
        if (fileStream is null) return NotFound();
        return File(fileStream, "application/octet-stream", fileDownloadName: Path.GetFileName(file.path));
    }

    [Route("{submissionGuid}/receptors")]
    [HttpGet]
    public async Task<IActionResult> GetReceptorsFile(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        if (submission is null) return NotFound();
        var file = await _fileService.GetFile(submission.receptorListFileId!);
        if (file is null) return NotFound();
        var fileStream = await _fileService.GetFileStream(submission.receptorListFileId!);
        if (fileStream is null) return NotFound();
        return File(fileStream, "application/octet-stream", fileDownloadName: Path.GetFileName(file.path));
    }
}
