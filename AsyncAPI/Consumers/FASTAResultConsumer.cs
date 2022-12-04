using System.Text.Json;

using AsyncAPI.Models;

using DataAccess.Repositories;

using HttpAPI.Models;

using MassTransit;

using StackExchange.Redis;

namespace AsyncAPI.Consumers;

public class FASTAResultConsumer : IConsumer<FASTAResult>
{
    private readonly IUserFileRepository _userFileRepository;
    private readonly IRepository<ReceptorFile> _receptorFileRepository;
    private readonly IConnectionMultiplexer _redis;

    public FASTAResultConsumer(IUserFileRepository userFileRepository,
                               IRepository<ReceptorFile> receptorFileRepository,
                               IConnectionMultiplexer redis)
    {
        _userFileRepository = userFileRepository;
        _receptorFileRepository = receptorFileRepository;
        _redis = redis;
    }

    public async Task Consume(ConsumeContext<FASTAResult> context)
    {
        var model = context.Message;

        var db = _redis.GetDatabase();
        var taskInfoRaw = await db.StringGetAsync("FASTA:" + model.id);
        if (!taskInfoRaw.HasValue) throw new FileNotFoundException();
    
        FASTATaskInfo? taskInfo = JsonSerializer.Deserialize<FASTATaskInfo>(taskInfoRaw.ToString());
        if (taskInfo is null) throw new FileNotFoundException();

        if (taskInfo.type == FASTATaskType.Receptor)
        {
            var receptor = await _receptorFileRepository.GetAsync(taskInfo.receptorId!);
            receptor!.FASTA = model.FASTA;
            await _receptorFileRepository.UpdateAsync(taskInfo.receptorId!, receptor!);
        }
        else if (taskInfo.type == FASTATaskType.UserFile)
        {
            var userFile = await _userFileRepository.GetAsync(taskInfo.userFileId!);
            userFile!.FASTA = model.FASTA;
            await _userFileRepository.UpdateAsync(taskInfo.userFileId!, userFile!);
        }
    }
}