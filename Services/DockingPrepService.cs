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
        if (receptor.fileId == null) 
        {
            _logger.LogError($"Trying to prepare receptor without file! Receptor id: {receptor.id}");
            return;
        }

        var db = _redis.GetDatabase();
        var guid = Guid.NewGuid();
        var taskInfo = new DockingPrepTaskInfo
        {
            type = EDockingPrepPeptideType.Receptor,
            receptorId = receptor.id
        };

        var taskInfoJSON = JsonSerializer.Serialize(taskInfo);
        await db.StringSetAsync("DockingPrep:" + guid.ToString(), taskInfoJSON);

        var file = await _fileService.GetFile(receptor.fileId);

        if (file == null)
        {
            _logger.LogError($"Receptor file not found! Receptor id: {receptor.id}");
            await db.StringGetDeleteAsync("DockingPrep:" + guid.ToString());
            return;
        }

        var task = new DockingPrepTask
        {
            id = guid,
            path = file.path,
            type = EDockingPrepPeptideType.Receptor
        };

        await _dockingPrepTaskPublisher.PublishDockingPrepTask(task);
    }

    public async Task PrepareForDocking(Submission submission)
    {
        if (submission.fileId == null) 
        {
            _logger.LogError($"Trying to prepare ligand without file! Submission id: {submission.guid}");
            return;
        }

        var db = _redis.GetDatabase();
        var guid = Guid.NewGuid();
        var taskInfo = new DockingPrepTaskInfo
        {
            type = EDockingPrepPeptideType.Ligand,
            submissionId = submission!.id
        };

        var taskInfoJSON = JsonSerializer.Serialize(taskInfo);
        await db.StringSetAsync("DockingPrep:" + guid.ToString(), taskInfoJSON);

        var file = await _fileService.GetFile(submission.fileId);

        if (file == null)
        {
            _logger.LogError($"Ligand file not found! Submission id: {submission.guid}");
            await db.StringGetDeleteAsync("DockingPrep:" + guid.ToString());
            return;
        }

        var task = new DockingPrepTask
        {
            id = guid,
            path = file!.path,
            type = EDockingPrepPeptideType.Ligand
        };

        await _dockingPrepTaskPublisher.PublishDockingPrepTask(task);
    }
}