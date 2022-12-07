using System.Net.Http.Headers;

using DataAccess.Repositories;

using HttpAPI.Models;

using StackExchange.Redis;
using AsyncAPI.Publishers;
using AsyncAPI.Models;
using System.Text.Json;

namespace Services;

class PDBFixService : IPDBFixService
{
    private readonly ILogger<PDBFixService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IPDBFixTaskPublisher _taskPublisher;
    private readonly IFileService _fileService;
    private readonly IConnectionMultiplexer _redis;

    
    public PDBFixService(ILogger<PDBFixService> logger,
                         IConfiguration configuration, IPDBFixTaskPublisher taskPublisher,
                         IConnectionMultiplexer redis, IFileService fileService)
    {
        _logger = logger;
        _configuration = configuration;
        _taskPublisher = taskPublisher;
        _redis = redis;
        _fileService = fileService;
    }

    public async Task PublishPDBFixTask(Submission submission)
    {
        var db = _redis.GetDatabase();
        var guid = Guid.NewGuid();
        var taskInfo = new PDBFixTaskInfo
        {
            submissionId = submission.id
        };

        var taskInfoJSON = JsonSerializer.Serialize(taskInfo);
        await db.StringSetAsync("PDBFix:" + guid.ToString(), taskInfoJSON);

        var file = await _fileService.GetFile(submission.fileId!);

        var task = new PDBFixTask
        {
            id = guid,
            path = file!.path
        };

        await _taskPublisher.PublishPDBFixTask(task);
    }
}