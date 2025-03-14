using Microsoft.AspNetCore.Mvc;
using tuskar.statisticApp.Models;

namespace tuskar.statisticApp.Controllers;

[ApiController]
public class InternalController(ILogger<InternalController> logger) : ControllerBase
{
    [HttpGet("ping")]
    public async Task<ActionResult<Internal.PingResponse>> Get()
    {
        logger.LogInformation("Ping received");

        await Task.Delay(2000);
        var pong = new Internal.PingResponse
        {
            result = "pong"
        };
        return Ok(pong);
    }
}