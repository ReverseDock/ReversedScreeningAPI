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
    private readonly IConfiguration _configuration;

    public SubmissionController(ILogger<SubmissionController> logger,
                                ISubmissionService submissionService,
                                IDockingPrepService dockingPrepService,
                                IFileService fileService, IPDBFixService pdbFixService,
                                IFASTAService fastaService, IReceptorService receptorService,
                                IConfiguration configuration)
    {
        _logger = logger;
        _submissionService = submissionService;
        _dockingPrepService = dockingPrepService;
        _fileService = fileService;
        _pdbFixService = pdbFixService;
        _fastaService = fastaService;
        _receptorService = receptorService;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<ActionResult> CreateSubmission(IFormFile ligandFile)
    {
        var userIP = Request.HttpContext.Connection.RemoteIpAddress;
        var submission = await _submissionService.CreateSubmission(ligandFile, userIP!.ToString());
        await _pdbFixService.PublishPDBFixTask(submission);
        await _fastaService.PublishFASTATask(submission);
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
        var uniProtIds = await _submissionService.GetUniProtIdsFromSubmission(submission!.id!);
        var maxReceptors = int.Parse(_configuration.GetSection("Limitations")["MaxReceptorAmount"]);
        if (uniProtIds.Count() > maxReceptors) return BadRequest($"Too many UniProtId's provided. Limit is {maxReceptors}.");
        var receptorsDTO = await _receptorService.GetReceptorStatusDTOs(uniProtIds);
        return Ok(receptorsDTO);
    }

    [HttpPost]
    [Route("{submissionGuid}/receptors/confirm")]
    public async Task<ActionResult> ConfirmReceptors(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        return Ok(submission!.confirmationGuid);
    }

    [HttpPost]
    [Route("{submissionGuid}/confirm/{confirmationGuid}")]
    public async Task<ActionResult> ConfirmSubmission(Guid submissionGuid, Guid confirmationGuid)
    {
        await _submissionService.ConfirmSubmission(submissionGuid, confirmationGuid);
        var submission = await _submissionService.GetSubmission(submissionGuid);
        await _dockingPrepService.PrepareForDocking(submission!);
        return Ok();
    }

    [HttpGet]
    [Route("{submissionGuid}/results")]
    public async Task<ActionResult> GetResults(Guid submissionGuid)
    {
        var submission = await _submissionService.GetSubmission(submissionGuid);
        var results = await _submissionService.GetResults(submissionGuid);
        var dto = new SubmissionInfoDTO
        {
            ligandFASTA = submission!.FASTA,
            dockingResults = results
        };
        return Ok(dto);
    }

    [HttpGet]
    [Route("{submissionGuid}/results/{resultGuid}")]
    public async Task<IActionResult> GetResultFile(Guid submissionGuid, Guid resultGuid)
    {
        var file = await _submissionService.GetResultFile(submissionGuid, resultGuid);
        var fileStream = await _fileService.GetFileStream(file!.id!);
        if (fileStream is null) return NotFound();
        return File(fileStream, "application/octet-stream", fileDownloadName: Path.GetFileName(file!.path));
    }

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
}
