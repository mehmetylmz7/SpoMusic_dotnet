using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SpoMusic.Api.Data;

namespace SpoMusic.Api.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _db;
    private static readonly DateTime _startTime = DateTime.UtcNow;

    public HealthController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("api/status")]
    public IActionResult GetRoot()
    {
        return Ok(new { message = "SpoMusic .NET 9 API is running!", version = "1.0.0" });
    }

    [HttpGet("health")]
    public async Task<IActionResult> CheckHealth()
    {
        string dbStatus = "down";
        try
        {
            if (await _db.Database.CanConnectAsync())
            {
                dbStatus = "up";
            }
        }
        catch
        {
            dbStatus = "down";
        }

        var isHealthy = dbStatus == "up";
        var health = new
        {
            status = isHealthy ? "ok" : "degraded",
            timestamp = DateTime.UtcNow.ToString("o"),
            uptimeSeconds = (DateTime.UtcNow - _startTime).TotalSeconds,
            services = new
            {
                database = dbStatus,
                api = "up"
            }
        };

        if (!isHealthy)
        {
            return StatusCode(503, health);
        }

        return Ok(health);
    }
}
