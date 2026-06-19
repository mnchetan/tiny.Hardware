using Microsoft.AspNetCore.Mvc;
using System.Text;
using tiny.Hardware.Api.DataObjects;
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

        [HttpPost("Write/{key}")]
        public async Task<IActionResult> Write(string key, [FromBody] HardwareWritePayload payload)
        {
            Global.LogDebug($"Received Write Request for Hardware Config Key: {key}");

            if (string.IsNullOrWhiteSpace(payload?.Data))
                return BadRequest("Payload data cannot be empty.");

            byte[] bytesToWrite;

            // Handle encoding mapping
            try
            {
                bytesToWrite = payload.EncodingFormat.ToUpperInvariant() switch
                {
                    "HEX" => ConvertFromHex(payload.Data),
                    "UTF8" => Encoding.UTF8.GetBytes(payload.Data),
                    "ASCII" => Encoding.ASCII.GetBytes(payload.Data),
                    _ => Encoding.ASCII.GetBytes(payload.Data)
                };
            }
            catch (Exception ex)
            {
                return BadRequest($"Invalid payload format for encoding {payload.EncodingFormat}. Error: {ex.Message}");
            }

            bool success = await _orchestrator.WriteToDeviceAsync(key, bytesToWrite);

            if (success)
                return Ok(new { message = $"Command successfully dispatched to {key}." });
            else
                return StatusCode(500, new { message = $"Failed to dispatch command to {key}. Ensure device is Started." });
        }

        // Helper for Modbus/PLC hex strings
        private byte[] ConvertFromHex(string hex)
        {
            hex = hex.Replace("-", "").Replace(" ", "");
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }
    }
}