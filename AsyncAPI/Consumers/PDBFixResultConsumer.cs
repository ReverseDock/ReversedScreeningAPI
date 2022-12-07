using System.Text.Json;

using AsyncAPI.Models;

using DataAccess.Repositories;

using Services;

using MassTransit;

using StackExchange.Redis;

namespace AsyncAPI.Consumers;

public class PDBFixResultConsumer : IConsumer<PDBFixResult>
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IConnectionMultiplexer _redis;
    private readonly IFileService _fileService;

    public PDBFixResultConsumer(ISubmissionRepository submissionRepository,
                               IConnectionMultiplexer redis,
                               IFileService fileService)
    {
        _submissionRepository = submissionRepository;
        _redis = redis;
        _fileService = fileService;
    }

    public async Task Consume(ConsumeContext<PDBFixResult> context)
    {
        var model = context.Message;

        var db = _redis.GetDatabase();
        var taskInfoRaw = await db.StringGetAsync("PDBFix:" + model.id);
        if (!taskInfoRaw.HasValue) throw new FileNotFoundException();
    
        PDBFixTaskInfo? taskInfo = JsonSerializer.Deserialize<PDBFixTaskInfo>(taskInfoRaw.ToString());
        if (taskInfo is null) throw new FileNotFoundException();

        var submission = await _submissionRepository.GetAsync(taskInfo.submissionId!);

        var fixedFile = await _fileService.CreateFile(model.path, true);
        submission!.fixedFileId = fixedFile!.id;
        submission!.fixedJSONResult = model.JSONResult;

        await _submissionRepository.UpdateAsync(taskInfo.submissionId!, submission!);
    }
}