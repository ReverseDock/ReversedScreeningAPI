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
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IReceptorRepository _receptorRepository;
    private readonly IConnectionMultiplexer _redis;
    private readonly IConfiguration _configuration;
    private readonly IDockingPrepService _dockingPrepService;

    public FASTAResultConsumer(ISubmissionRepository submissionRepository,
                               IReceptorRepository receptorRepository,
                               IConnectionMultiplexer redis, IConfiguration configuration,
                               IDockingPrepService dockingPrepService)
    {
        _submissionRepository = submissionRepository;
        _receptorRepository = receptorRepository;
        _redis = redis;
        _configuration = configuration;
        _dockingPrepService = dockingPrepService;
    }

    public async Task Consume(ConsumeContext<FASTAResult> context)
    {
        var model = context.Message;

        var db = _redis.GetDatabase();
        var taskInfoRaw = await db.StringGetAsync("FASTA:" + model.id);
        if (!taskInfoRaw.HasValue) throw new FileNotFoundException();
    
        FASTATaskInfo? taskInfo = JsonSerializer.Deserialize<FASTATaskInfo>(taskInfoRaw.ToString());
        if (taskInfo is null) throw new FileNotFoundException();

        if (taskInfo.type == FASTATaskType.Receptor)
        {
            var receptor = await _receptorRepository.GetAsync(taskInfo.receptorId!);
            receptor!.FASTA = model.FASTA;
            var maxSize = int.Parse(_configuration.GetSection("Limitations")["MaxReceptorSize"]);
            if (model.FASTA.Length > maxSize)
            {
                receptor!.status = ReceptorFileStatus.TooBig;
                await _receptorRepository.UpdateAsync(taskInfo.receptorId!, receptor!);
                return;
            }
            await _receptorRepository.UpdateAsync(taskInfo.receptorId!, receptor!);
            await _dockingPrepService.PrepareForDocking(receptor);
        }
        else if (taskInfo.type == FASTATaskType.Ligand)
        {
            var submission = await _submissionRepository.GetAsync(taskInfo.submissionId!);
            submission!.FASTA = model.FASTA;
            await _submissionRepository.UpdateAsync(taskInfo.submissionId!, submission!);
        }
    }
}