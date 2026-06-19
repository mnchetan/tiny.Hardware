using Microsoft.AspNetCore.Mvc;
using tiny.Hardware.Core.Configurations;
using tiny.Hardware.Core.DataObjects;
using tiny.Hardware.Core.Engine;
using tiny.WebApi.DataObjects;

namespace tiny.Hardware.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HardwareController : Controller
    {
        private readonly HardwareOrchestrator _orchestrator;
        private readonly ITinyHardwareConfigurations _configurations;

        // Injecting the hardware-specific configs we mapped in Program.cs
        public HardwareController(HardwareOrchestrator orchestrator, ITinyHardwareConfigurations configurations)
        {
            _orchestrator = orchestrator;
            _configurations = configurations;
        }

        [HttpGet("Start/{key}")]
        public async Task<IActionResult> Start(string key)
        {
            // We can still use Global from the NuGet package for logging!
            Global.LogDebug($"Received Start Request for Hardware Config Key: {key}");

            if (!_configurations.HardwareSpecifications.TryGetValue(key, out HardwareSpecification? config))
            {
                Global.LogError($"Hardware Configuration for key {key} not found.", null);
                return NotFound($"Hardware Configuration for key {key} not found.");
            }

            await _orchestrator.StartMonitoringAsync(key, config);
            return Ok(new { message = $"Successfully connected and monitoring {key}." });
        }

        [HttpGet("Stop/{key}")]
        public IActionResult Stop(string key)
        {
            _orchestrator.StopMonitoring(key);
            return Ok(new { message = $"Stopped monitoring {key}." });
        }
    }
}