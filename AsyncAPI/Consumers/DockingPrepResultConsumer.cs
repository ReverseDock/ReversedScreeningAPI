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
    private readonly IUserFileRepository _userFileRepository;
    private readonly IReceptorFileRepository _receptorFileRepository;
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IConnectionMultiplexer _redis;
    private readonly ISubmissionService _submissionService;

    public DockingPrepResultConsumer(IUserFileRepository userFileRepository,
                                     IReceptorFileRepository receptorFileRepository,
                                     IConnectionMultiplexer redis, ISubmissionService submissionService,
                                     ISubmissionRepository submissionRepository)
    {
        _userFileRepository = userFileRepository;
        _receptorFileRepository = receptorFileRepository;
        _redis = redis;
        _submissionService = submissionService;
        _submissionRepository = submissionRepository;
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
            if (model.fullPath is null)
            {
                await _receptorFileRepository.RemoveAsync(taskInfo.receptorId!);
                // leaves orphaned files!!
                return;
            }

            var receptor = await _receptorFileRepository.GetAsync(taskInfo.receptorId!);
            receptor!.fullPDBQTPath = model.fullPath;
            receptor!.fullConfigPath = model.fullConfigPath;
            await _receptorFileRepository.UpdateAsync(taskInfo.receptorId!, receptor!);
        }
        else if (taskInfo.type == EDockingPrepPeptideType.Ligand)
        {
            if (model.fullPath is null)
            {
                var submission = await _submissionRepository.GetAsync(taskInfo.submissionId!);
                submission!.failed = true;
                await _submissionRepository.UpdateAsync(taskInfo.submissionId!, submission);
                return;
            }

            var userFile = await _userFileRepository.GetAsync(taskInfo.userFileId!);
            userFile!.fullPDBQTPath = model.fullPath;
            await _userFileRepository.UpdateAsync(taskInfo.userFileId!, userFile!);
            await _submissionService.CreateDockings(taskInfo.submissionId!);
        }
    }
}