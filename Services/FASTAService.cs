using HttpAPI.Models;
using StackExchange.Redis;
using AsyncAPI.Publishers;
using AsyncAPI.Models;
using System.Text.Json;

namespace Services;

public class FASTAService : IFASTAService
{
    private readonly ILogger<FASTAService> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly IFASTATaskPublisher _fastaTaskPublisher;
    
    public FASTAService(ILogger<FASTAService> logger, IConnectionMultiplexer redis,
                        IFASTATaskPublisher fastaTaskPublisher)
    {
        _logger = logger;
        _redis = redis;
        _fastaTaskPublisher = fastaTaskPublisher;
    }

    public async Task PublishFASTATask(ReceptorFile? receptor = null, UserFile? userFile = null)
    {
        var db = _redis.GetDatabase();
        var guid = Guid.NewGuid();
        var taskInfo = new FASTATaskInfo
        {
            type = receptor == null ? FASTATaskType.UserFile : FASTATaskType.Receptor,
            receptorId = receptor != null ? receptor.id : null,
            userFileId = userFile != null ? userFile.id : null
        };

        var taskInfoJSON = JsonSerializer.Serialize(taskInfo);
        await db.StringSetAsync("FASTA:" + guid.ToString(), taskInfoJSON);

        var task = new FASTATask
        {
            id = guid,
            fullPath = receptor == null ? userFile!.fullPath : receptor.fullPath
        };

        await _fastaTaskPublisher.PublishFASTATask(task);
    }
}