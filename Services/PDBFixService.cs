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
    private readonly IRepository<ReceptorFile> _fileRepository;
    private readonly IConfiguration _configuration;
    private readonly IPDBFixTaskPublisher _taskPublisher;
    private readonly IConnectionMultiplexer _redis;

    
    public PDBFixService(ILogger<PDBFixService> logger, IRepository<ReceptorFile> fileRepository,
                               IConfiguration configuration, IPDBFixTaskPublisher taskPublisher,
                               IConnectionMultiplexer redis)
    {
        _logger = logger;
        _fileRepository = fileRepository;
        _configuration = configuration;
        _taskPublisher = taskPublisher;
        _redis = redis;
    }

    public async Task PublishPDBFixTask(UserFile userFile)
    {
        var db = _redis.GetDatabase();
        var guid = Guid.NewGuid();
        var taskInfo = new PDBFixTaskInfo
        {
            userFileId = userFile.id
        };

        var taskInfoJSON = JsonSerializer.Serialize(taskInfo);
        await db.StringSetAsync("PDBFix:" + guid.ToString(), taskInfoJSON);

        var task = new PDBFixTask
        {
            id = guid,
            fullPath = userFile.fullPath
        };

        await _taskPublisher.PublishPDBFixTask(task);
    }
}