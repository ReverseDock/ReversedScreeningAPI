using Microsoft.AspNetCore.Mvc;
using Models;
using AsyncAPI.Publishers;

namespace HttpAPI;

[ApiController]
[Route("Dockings")]
public class DockingPublishController : ControllerBase
{

    private readonly ILogger<DockingPublishController> _logger;
    private readonly IDockingPublisher _dockingPublisher;

    public DockingPublishController(ILogger<DockingPublishController> logger,
                                    IDockingPublisher dockingPublisher)
    {
        _logger = logger;
        _dockingPublisher = dockingPublisher;
    }

    [HttpGet(Name = "PublishDockingTest")]
    public async Task<ActionResult> PublishDockingTest() 
    {
        var docking = new Docking { Text = "lol" };
        await _dockingPublisher.PublishDocking(docking);
        return Ok();
    }
}