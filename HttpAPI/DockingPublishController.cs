using Microsoft.AspNetCore.Mvc;
using Models;
using AsyncAPI.Publishers;
using DataAccess.Repositories;

namespace HttpAPI;

[ApiController]
[Route("Dockings")]
public class DockingPublishController : ControllerBase
{

    private readonly ILogger<DockingPublishController> _logger;
    private readonly IDockingPublisher _dockingPublisher;
    private readonly ISubmissionRepository _submissionRepository;

    public DockingPublishController(ILogger<DockingPublishController> logger,
                                    IDockingPublisher dockingPublisher,
                                    ISubmissionRepository submissionRepository)
    {
        _logger = logger;
        _dockingPublisher = dockingPublisher;
        _submissionRepository = submissionRepository;
    }

    [HttpGet(Name = "PublishDockingTest")]
    public async Task<ActionResult> PublishDockingTest() 
    {
        var docking = new Docking { Text = "lol" };
        await _dockingPublisher.PublishDocking(docking);
        return Ok();
    }

    [HttpPost(Name = "TestMongoDBAccess")]
    public async Task<ActionResult> TestMongoDBAccess()
    {
        await _submissionRepository.CreateAsync(new Submission { path = "lol"} );
        var result = await _submissionRepository.GetAsync();
        return Ok(result);
    }
}