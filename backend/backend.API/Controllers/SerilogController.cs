using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Globalization;

namespace Backend.API.Controllers;

[ApiController]
[Route("logs/serilog")]
public sealed class SerilogController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public SerilogController(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    [HttpGet]
    public IActionResult Get()
    {
        Log.Information("Serilog controller invoked.");
        return Ok("Okay");
    }
}
