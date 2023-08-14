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

    public async Task PublishFASTATask(Submission submission, Receptor receptor)
    {
        var db = _redis.GetDatabase();
        var guid = Guid.NewGuid();
        var taskInfo = new FASTATaskInfo
        {
            submissionId = submission.id!,
            receptorGuid = receptor.guid!
        };

        var taskInfoJSON = JsonSerializer.Serialize(taskInfo);
        await db.StringSetAsync("FASTA:" + guid.ToString(), taskInfoJSON);

        var file = receptor.file;

        var task = new FASTATask
        {
            id = guid,
            path = file!.path
        };

        await _fastaTaskPublisher.PublishFASTATask(task);
    }
}