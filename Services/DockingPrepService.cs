using HttpAPI.Models;
using StackExchange.Redis;
using AsyncAPI.Publishers;
using AsyncAPI.Models;
using System.Text.Json;

namespace Services;

public class DockingPrepService : IDockingPrepService
{
    private readonly ILogger<DockingPrepService> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly IDockingPrepTaskPublisher _dockingPrepTaskPublisher;
    private readonly IFileService _fileService;
    
    public DockingPrepService(ILogger<DockingPrepService> logger, IConnectionMultiplexer redis,
                              IDockingPrepTaskPublisher dockingPrepTaskPublisher,
                        IFileService fileService)
    {
        _logger = logger;
        _redis = redis;
        _dockingPrepTaskPublisher = dockingPrepTaskPublisher;
        _fileService = fileService;
    }

    public async Task PrepareForDocking(Receptor receptor)
    {
        var db = _redis.GetDatabase();
        var guid = Guid.NewGuid();
        var taskInfo = new DockingPrepTaskInfo
        {
            type = EDockingPrepPeptideType.Receptor,
            receptorId = receptor.id
        };

        var taskInfoJSON = JsonSerializer.Serialize(taskInfo);
        await db.StringSetAsync("DockingPrep:" + guid.ToString(), taskInfoJSON);

        var file = await _fileService.GetFile(receptor.fileId!);

        var task = new DockingPrepTask
        {
            id = guid,
            path = file!.path,
            type = EDockingPrepPeptideType.Receptor
        };

        await _dockingPrepTaskPublisher.PublishDockingPrepTask(task);
    }

    public async Task PrepareForDocking(Submission submission)
    {
        var db = _redis.GetDatabase();
        var guid = Guid.NewGuid();
        var taskInfo = new DockingPrepTaskInfo
        {
            type = EDockingPrepPeptideType.Ligand,
            submissionId = submission!.id
        };

        var taskInfoJSON = JsonSerializer.Serialize(taskInfo);
        await db.StringSetAsync("DockingPrep:" + guid.ToString(), taskInfoJSON);

        var file = await _fileService.GetFile(submission!.fixedFileId!);

        var task = new DockingPrepTask
        {
            id = guid,
            path = file!.path,
            type = EDockingPrepPeptideType.Ligand
        };

        await _dockingPrepTaskPublisher.PublishDockingPrepTask(task);
    }
}