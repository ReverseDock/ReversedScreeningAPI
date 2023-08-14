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

    public async Task PrepareForDocking(Submission submission, Receptor receptor)
    {
        if (receptor.file == null) 
        {
            _logger.LogError($"Trying to prepare receptor without file! Submission id: {submission.guid}");
            return;
        }

        var db = _redis.GetDatabase();
        var guid = Guid.NewGuid();
        var taskInfo = new DockingPrepTaskInfo
        {
            type = EDockingPrepPeptideType.Receptor,
            receptorGuid = receptor.guid,
            submissionId = submission!.id
        };

        var taskInfoJSON = JsonSerializer.Serialize(taskInfo);
        await db.StringSetAsync("DockingPrep:" + guid.ToString(), taskInfoJSON);

        var task = new DockingPrepTask
        {
            id = guid,
            path = receptor.file.path,
            type = EDockingPrepPeptideType.Receptor
        };

        await _dockingPrepTaskPublisher.PublishDockingPrepTask(task);
    }

    public async Task PrepareForDocking(Submission submission)
    {
        if (submission.ligand == null) 
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

        var task = new DockingPrepTask
        {
            id = guid,
            path = submission.ligand.path,
            type = EDockingPrepPeptideType.Ligand
        };

        await _dockingPrepTaskPublisher.PublishDockingPrepTask(task);
    }
}