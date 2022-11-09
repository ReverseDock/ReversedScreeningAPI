using Microsoft.AspNetCore.Mvc;
using System;
using EasyNetQ;
using Models;

using System.Text;
namespace HttpAPI;

[ApiController]
[Route("Dockings")]
public class DockingPublishController : ControllerBase
{

    private readonly ILogger<DockingPublishController> _logger;
    private readonly IAdvancedBus _bus;

    public DockingPublishController(ILogger<DockingPublishController> logger,
                                    IAdvancedBus bus)
    {
        _logger = logger;
        _bus = bus;
    }

    [HttpGet(Name = "PublishDockingTest")]
    public async Task<ActionResult> PublishDockingTest() 
    {
        var dockings = new Docking { Text = "Gday m8" };
        var message = new Message<Docking>(dockings);
        var routingKey = "Docking";

        var queue = await _bus.QueueDeclareAsync("q.dockings", true, false, false);
        var exchange = await _bus.ExchangeDeclareAsync("e.dockings", "direct");
        var binding = await _bus.BindAsync(exchange, queue, routingKey);

        await _bus.PublishAsync(exchange, routingKey, true, message);
        return Ok();
    }
}