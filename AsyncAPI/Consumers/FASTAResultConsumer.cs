using System.Text.Json;

using AsyncAPI.Models;

using DataAccess.Repositories;

using HttpAPI.Models;

using MassTransit;
using Services;
using StackExchange.Redis;

namespace AsyncAPI.Consumers;

public class FASTAResultConsumer : IConsumer<FASTAResult>
{
    private readonly ILogger<FASTAResultConsumer> _logger;
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IReceptorRepository _receptorRepository;
    private readonly IConnectionMultiplexer _redis;
    private readonly IConfiguration _configuration;
    private readonly IDockingPrepService _dockingPrepService;
    private readonly IFileService _fileService;

    public FASTAResultConsumer(ILogger<FASTAResultConsumer> logger,
                               ISubmissionRepository submissionRepository,
                               IReceptorRepository receptorRepository,
                               IConnectionMultiplexer redis, IConfiguration configuration,
                               IDockingPrepService dockingPrepService,
                               IFileService fileService)
    {
        _logger = logger;
        _submissionRepository = submissionRepository;
        _receptorRepository = receptorRepository;
        _redis = redis;
        _configuration = configuration;
        _dockingPrepService = dockingPrepService;
        _fileService = fileService;
    }

    public async Task Consume(ConsumeContext<FASTAResult> context)
    {
        var model = context.Message;

        var db = _redis.GetDatabase();
        var taskInfoRaw = await db.StringGetDeleteAsync("FASTA:" + model.id);
        if (!taskInfoRaw.HasValue)
        {
            _logger.LogError($"No FASTATaskInfo found for {model.id}");
            return;
        }
    
        FASTATaskInfo? taskInfo = JsonSerializer.Deserialize<FASTATaskInfo>(taskInfoRaw.ToString());
                if (taskInfo is null)
        {
            _logger.LogError($"Could not deserialize FASTATaskInfo for {model.id}. Raw: {taskInfoRaw.ToString()}");
            return;
        }

        var receptor = await _receptorRepository.GetAsync(taskInfo.receptorId);
        if (receptor is null)
        {
            _logger.LogError($"Could not find receptor of FASTATask. Receptor: {taskInfo.receptorId}");
            return;
        }

        receptor.FASTA = model.FASTA;
        var maxSize = int.Parse(_configuration.GetSection("Limitations")["MaxReceptorSize"]);

        if (model.FASTA.Length > maxSize)
        {
            receptor.status = ReceptorFileStatus.TooBig;
            await _fileService.RemoveFile(receptor.fileId!);
            receptor.fileId = null;
            await _receptorRepository.UpdateAsync(taskInfo.receptorId, receptor);
            return;
        }
        await _receptorRepository.UpdateAsync(taskInfo.receptorId, receptor);

        await _dockingPrepService.PrepareForDocking(receptor);
    }
}