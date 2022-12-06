using System.Text.Json;

using AsyncAPI.Models;

using DataAccess.Repositories;

using HttpAPI.Models;

using MassTransit;

using StackExchange.Redis;

namespace AsyncAPI.Consumers;

public class PDBFixResultConsumer : IConsumer<PDBFixResult>
{
    private readonly IUserFileRepository _userFileRepository;
    private readonly IRepository<ReceptorFile> _receptorFileRepository;
    private readonly IConnectionMultiplexer _redis;

    public PDBFixResultConsumer(IUserFileRepository userFileRepository,
                               IRepository<ReceptorFile> receptorFileRepository,
                               IConnectionMultiplexer redis)
    {
        _userFileRepository = userFileRepository;
        _receptorFileRepository = receptorFileRepository;
        _redis = redis;
    }

    public async Task Consume(ConsumeContext<PDBFixResult> context)
    {
        var model = context.Message;

        var db = _redis.GetDatabase();
        var taskInfoRaw = await db.StringGetAsync("PDBFix:" + model.id);
        if (!taskInfoRaw.HasValue) throw new FileNotFoundException();
    
        PDBFixTaskInfo? taskInfo = JsonSerializer.Deserialize<PDBFixTaskInfo>(taskInfoRaw.ToString());
        if (taskInfo is null) throw new FileNotFoundException();

        var userFile = await _userFileRepository.GetAsync(taskInfo.userFileId!);
        userFile!.fullFixedPath = model.fullPath;
        userFile!.fixedJSONResult = model.JSONResult;
        await _userFileRepository.UpdateAsync(taskInfo.userFileId!, userFile!);
    }
}