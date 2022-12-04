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
    private readonly IFASTATaskPublisher _fastaTaskPublisher;
    private readonly IDockingPrepTaskPublisher _dockingPrepTaskPublisher;
    
    public DockingPrepService(ILogger<DockingPrepService> logger, IConnectionMultiplexer redis,
                        IFASTATaskPublisher fastaTaskPublisher, IDockingPrepTaskPublisher dockingPrepTaskPublisher)
    {
        _logger = logger;
        _redis = redis;
        _fastaTaskPublisher = fastaTaskPublisher;
        _dockingPrepTaskPublisher = dockingPrepTaskPublisher;
    }

    public async Task PrepareForDocking(ReceptorFile? receptor = null, UserFile? userFile = null,
                                        Submission? submission = null)
    {
        var db = _redis.GetDatabase();
        var guid = Guid.NewGuid();
        var taskInfo = new DockingPrepTaskInfo
        {
            type = receptor == null ? EDockingPrepPeptideType.Ligand : EDockingPrepPeptideType.Receptor,
            receptorId = receptor != null ? receptor.id : null,
            userFileId = userFile != null ? userFile.id : null,
            submissionId = receptor == null ? submission!.id : null
        };

        var taskInfoJSON = JsonSerializer.Serialize(taskInfo);
        await db.StringSetAsync("DockingPrep:" + guid.ToString(), taskInfoJSON);

        var task = new DockingPrepTask
        {
            id = guid,
            fullPath = receptor == null ? userFile!.fullFixedPath : receptor!.fullPath,
            type = receptor == null ? EDockingPrepPeptideType.Ligand : EDockingPrepPeptideType.Receptor
        };

        await _dockingPrepTaskPublisher.PublishDockingPrepTask(task);
    }
}