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
    private readonly IAlphaFoldReceptorRepository _alphaFoldReceptorRepository;
    private readonly IConnectionMultiplexer _redis;
    private readonly IConfiguration _configuration;
    private readonly IDockingPrepService _dockingPrepService;
    private readonly IFileService _fileService;

    public FASTAResultConsumer(ILogger<FASTAResultConsumer> logger,
                               ISubmissionRepository submissionRepository,
                               IAlphaFoldReceptorRepository alphaFoldReceptorRepository,
                               IConnectionMultiplexer redis, IConfiguration configuration,
                               IDockingPrepService dockingPrepService,
                               IFileService fileService)
    {
        _logger = logger;
        _submissionRepository = submissionRepository;
        _alphaFoldReceptorRepository = alphaFoldReceptorRepository;
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

        if (taskInfo.type == FASTATaskType.UserPDB) 
        {
            var submission = await _submissionRepository.GetAsync(taskInfo.submissionId);
            if (submission is null)
            {
                _logger.LogError($"Could not find submission of FASTATask. Submission: {taskInfo.submissionId}");
                return;
            }

            var receptor = submission.receptors.FirstOrDefault(r => r.guid == taskInfo.receptorGuid);
            if (receptor is null)
            {
                _logger.LogError($"Could not find receptor of FASTATask. Receptor: {taskInfo.receptorGuid}");
                return;
            }

            receptor.FASTA = model.FASTA;
            var maxSize = int.Parse(_configuration.GetSection("Limitations")["MaxReceptorSize"]);

            if (model.FASTA.Length > maxSize)
            {
                receptor.status = ReceptorFileStatus.TooBig;
                receptor.affinity = -1;
                _fileService.RemoveFile(receptor.file!);
                receptor.file = null;
                await _submissionRepository.UpdateReceptor(submission, receptor);
                return;
            }

            await _dockingPrepService.PrepareForDocking(submission, receptor);
        } else 
        {
            var receptor = await _alphaFoldReceptorRepository.GetByUnitProtID(taskInfo.UnitProtID);
            if (receptor is null)
            {
                _logger.LogError($"Could not find AlphaFold receptor of FASTATask. Receptor: {taskInfo.UnitProtID}");
                return;
            }

            receptor.FASTA = model.FASTA;
            await _alphaFoldReceptorRepository.UpdateAsync(receptor.id!, receptor);
        }
    }
}