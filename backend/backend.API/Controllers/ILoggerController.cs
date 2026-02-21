using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.API.Controllers;

[ApiController]
[Route("logs/ilogger")]
public sealed class ILoggerController : ControllerBase
{
    private readonly ILogger<ILoggerController> _logger;

    public ILoggerController(ILogger<ILoggerController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("ILogger controller invoked.");
        return Ok(new { status = "ok", logger = "ilogger" });
    }
}
