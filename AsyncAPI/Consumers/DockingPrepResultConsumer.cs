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
    private readonly IReceptorRepository _receptorRepository;
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IConnectionMultiplexer _redis;
    private readonly ISubmissionService _submissionService;
    private readonly IFileService _fileService;

    public DockingPrepResultConsumer(IReceptorRepository receptorRepository,
                                     IConnectionMultiplexer redis, ISubmissionService submissionService,
                                     ISubmissionRepository submissionRepository,
                                     IFileService fileService)
    {
        _receptorRepository = receptorRepository;
        _redis = redis;
        _submissionService = submissionService;
        _submissionRepository = submissionRepository;
        _fileService = fileService;
    }

    public async Task Consume(ConsumeContext<DockingPrepResult> context)
    {
        var model = context.Message;

        var db = _redis.GetDatabase();
        var taskInfoRaw = await db.StringGetAsync("DockingPrep:" + model.id);
        if (!taskInfoRaw.HasValue) throw new FileNotFoundException();
    
        DockingPrepTaskInfo? taskInfo = JsonSerializer.Deserialize<DockingPrepTaskInfo>(taskInfoRaw.ToString());
        if (taskInfo is null) throw new FileNotFoundException();

        if (taskInfo.type == EDockingPrepPeptideType.Receptor)
        {
            var receptor = await _receptorRepository.GetAsync(taskInfo.receptorId!);
            if (model.path is null)
            {
                if (receptor!.status != ReceptorFileStatus.TooBig)
                    receptor!.status = ReceptorFileStatus.PDBQTError;
                await _receptorRepository.UpdateAsync(taskInfo.receptorId!, receptor!);
                // Leaves orphaned files!
                return;
            }

            var pdbqtFile = await _fileService.CreateFile(model.path, false);
            var configFile = await _fileService.CreateFile(model.configPath, false);

            receptor!.status = ReceptorFileStatus.Ready;
            receptor!.pdbqtFileId = pdbqtFile!.id;
            receptor!.configFileId = configFile!.id;
            await _receptorRepository.UpdateAsync(taskInfo.receptorId!, receptor!);
        }
        else if (taskInfo.type == EDockingPrepPeptideType.Ligand)
        {
            var submission = await _submissionRepository.GetAsync(taskInfo.submissionId!);
            if (model.path is null)
            {
                
                submission!.status = SubmissionStatus.PreparationFailed;
                await _submissionRepository.UpdateAsync(taskInfo.submissionId!, submission);
                return;
            }

            var pdbqtFile = await _fileService.CreateFile(model.path, false);

            submission!.status = SubmissionStatus.PreparationComplete;
            submission!.pdbqtFileId = pdbqtFile!.id;

            await _submissionRepository.UpdateAsync(taskInfo.submissionId!, submission);
            await _submissionService.CreateDockings(taskInfo.submissionId!);
        }
    }
}