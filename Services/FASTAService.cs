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
    private readonly IFileService _fileService;
    
    public FASTAService(ILogger<FASTAService> logger, IConnectionMultiplexer redis,
                        IFASTATaskPublisher fastaTaskPublisher, IFileService fileService)
    {
        _logger = logger;
        _redis = redis;
        _fastaTaskPublisher = fastaTaskPublisher;
        _fileService = fileService;
    }

    public async Task PublishFASTATask(Receptor receptor)
    {
        var db = _redis.GetDatabase();
        var guid = Guid.NewGuid();
        var taskInfo = new FASTATaskInfo
        {
            receptorId = receptor.id!
        };

        var taskInfoJSON = JsonSerializer.Serialize(taskInfo);
        await db.StringSetAsync("FASTA:" + guid.ToString(), taskInfoJSON);

        var file = await _fileService.GetFile(receptor.fileId!);

        var task = new FASTATask
        {
            id = guid,
            path = file!.path
        };

        await _fastaTaskPublisher.PublishFASTATask(task);
    }
}