using System.Text.Json;

using AsyncAPI.Models;

using DataAccess.Repositories;

using HttpAPI.Models;

using MassTransit;

using StackExchange.Redis;

using Services;

namespace AsyncAPI.Consumers;

public class DockingPrepResultConsumer : IConsumer<DockingPrepResult>
{
    private readonly ILogger<DockingPrepResultConsumer> _logger;
    private readonly IReceptorRepository _receptorRepository;
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IConnectionMultiplexer _redis;
    private readonly ISubmissionService _submissionService;
    private readonly IFileService _fileService;
    private readonly IDockingPrepService _dockingPrepService;

    public DockingPrepResultConsumer(ILogger<DockingPrepResultConsumer> logger, IReceptorRepository receptorRepository,
                                     IConnectionMultiplexer redis, ISubmissionService submissionService,
                                     ISubmissionRepository submissionRepository,
                                     IFileService fileService, IDockingPrepService dockingPrepService)
    {
        _logger = logger;
        _receptorRepository = receptorRepository;
        _redis = redis;
        _submissionService = submissionService;
        _submissionRepository = submissionRepository;
        _fileService = fileService;
        _dockingPrepService = dockingPrepService;
    }

    public async Task Consume(ConsumeContext<DockingPrepResult> context)
    {
        var model = context.Message;

        var db = _redis.GetDatabase();
        var taskInfoRaw = await db.StringGetDeleteAsync("DockingPrep:" + model.id);
        if (!taskInfoRaw.HasValue)
        {
            _logger.LogError($"No DockingPrepTaskInfo found for {model.id}");
            return;
        }
    
        DockingPrepTaskInfo? taskInfo = JsonSerializer.Deserialize<DockingPrepTaskInfo>(taskInfoRaw.ToString());
        if (taskInfo is null)
        {
            _logger.LogError($"Could not deserialize DockingPrepTaskInfo for {model.id}. Raw: {taskInfoRaw.ToString()}");
            return;
        }

        if (taskInfo.type == EDockingPrepPeptideType.Receptor)
        {
            var receptor = await _receptorRepository.GetAsync(taskInfo.receptorId!);
            if (receptor is null)
            {
                _logger.LogError($"Could not find receptor of DockingPrepTask. Receptor: {taskInfo.receptorId}");
                return;
            }
            if (model.path is null)
            {
                if (receptor.status != ReceptorFileStatus.TooBig)
                {
                    receptor.status = ReceptorFileStatus.PDBQTError;
                    await _fileService.RemoveFile(receptor.fileId!);
                    receptor.fileId = null;
                    await _receptorRepository.UpdateAsync(taskInfo.receptorId!, receptor!);
                }
            } else {
                var pdbqtFile = await _fileService.CreateFile(model.path, "pdbqt", true);
                var configFile = await _fileService.CreateFile(model.configPath, "conf", true);

                receptor.status = ReceptorFileStatus.Ready;
                receptor.pdbqtFileId = pdbqtFile!.id;
                receptor.configFileId = configFile!.id;
                await _receptorRepository.UpdateAsync(taskInfo.receptorId!, receptor!);
            }

            // If no more unprocessed entries, trigger preparation of ligand
            var submission = await _submissionService.GetSubmission(taskInfo.submissionId!);
            if (submission is null)
            {
                _logger.LogError($"Could not find submission of DockingPrepTask. Submission: {taskInfo.receptorId}");
                return;
            }
            var unprocessed = await _submissionService.GetUnprocessedReceptors(submission);
            // Only prepare ligand if preparation not already completed or failed
            if (!unprocessed.Any() && submission.status != SubmissionStatus.PreparationComplete && submission.status != SubmissionStatus.PreparationFailed)
            {
                // Trigger preparation of ligand
                await _dockingPrepService.PrepareForDocking(submission);
            }
        }
        else if (taskInfo.type == EDockingPrepPeptideType.Ligand)
        {
            var submission = await _submissionRepository.GetAsync(taskInfo.submissionId!);

            if (submission is null)
            {
                _logger.LogError($"Could not find submission of DockingPrepTask. Submission: {taskInfo.receptorId}");
                return;
            }

            // Ligand already processed
            if (submission.status == SubmissionStatus.PreparationComplete || submission.status == SubmissionStatus.PreparationFailed)
            {
                return;
            }

            if (model.path is null)
            {
                submission.status = SubmissionStatus.PreparationFailed;
                await _submissionRepository.UpdateAsync(taskInfo.submissionId!, submission);
                return;
            }

            var pdbqtFile = await _fileService.CreateFile(model.path, "pdbqt", true);

            submission.status = SubmissionStatus.PreparationComplete;
            submission.pdbqtFileId = pdbqtFile!.id;

            await _submissionRepository.UpdateAsync(taskInfo.submissionId!, submission);
            await _submissionService.CreateDockings(submission);
        }
    }
}