using Microsoft.AspNetCore.Mvc;
using System.Text;
using tiny.Hardware.Api.DataObjects;
using tiny.Hardware.Core.Configurations;
using tiny.Hardware.Core.DataObjects;
using tiny.Hardware.Core.Engine;
using tiny.WebApi.DataObjects;
namespace tiny.Hardware.Api.Controllers
{
    /// <summary>
    /// API Controller for managing hardware interactions. This controller provides endpoints to start monitoring a hardware device, stop monitoring, and write commands to the device. It leverages the HardwareOrchestrator for handling the underlying logic and uses the configurations defined in ITinyHardwareConfigurations to manage different hardware specifications.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the HardwareController class with the specified orchestrator and configurations. This constructor sets up the necessary dependencies for the controller to function, allowing it to manage hardware interactions based on the provided configurations.
    /// </remarks>
    /// <param name="orchestrator"></param>
    /// <param name="configurations"></param>
    [ApiController]
    [Route("api/[controller]")]
    public class HardwareController(HardwareOrchestrator orchestrator, ITinyHardwareConfigurations configurations) : Controller
    {
        /// <summary>
        /// The HardwareOrchestrator is responsible for managing the lifecycle of hardware interactions, including starting and stopping monitoring sessions and writing commands to devices. It abstracts away the complexities of handling different hardware types and provides a unified interface for the controller to interact with.
        /// </summary>
        private readonly HardwareOrchestrator _orchestrator = orchestrator;
        /// <summary>
        /// ITinyHardwareConfigurations provides access to the hardware specifications and configurations defined for the application. This allows the controller to retrieve the necessary information about each hardware device based on a key, enabling dynamic handling of multiple hardware types without hardcoding specific logic for each one.
        /// </summary>
        private readonly ITinyHardwareConfigurations _configurations = configurations;
        /// <summary>
        /// Starts monitoring a hardware device based on the provided key. The key is used to look up the corresponding hardware specification from the configurations. If the configuration is found, the orchestrator is instructed to start monitoring the device using the specified settings. If the configuration is not found, a 404 Not Found response is returned. This endpoint allows clients to initiate interactions with different hardware devices dynamically by simply providing the appropriate key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet("Start/{key}")]
        [HttpOptions]
        public async Task<IActionResult> Start(string key)
        {
            // We can still use Global from the NuGet package for logging!
            Global.LogDebug($"Received Start Request for Hardware Config Key: {key}");
            if (!_configurations.HardwareSpecifications.TryGetValue(key, out HardwareSpecification config))
            {
                Global.LogError($"Hardware Configuration for key {key} not found.", null);
                return NotFound($"Hardware Configuration for key {key} not found.");
            }
            await _orchestrator.StartMonitoringAsync(key, config);
            return Ok(new { message = $"Successfully connected and monitoring {key}." });
        }
        /// <summary>
        /// Stops monitoring a hardware device based on the provided key. This endpoint allows clients to terminate interactions with a specific hardware device by instructing the orchestrator to stop monitoring it. The key is used to identify which device's monitoring session should be stopped. After the orchestrator processes the stop request, a confirmation message is returned to the client indicating that monitoring for the specified key has been stopped.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet("Stop/{key}")]
        [HttpOptions]
        public IActionResult Stop(string key)
        {
            _orchestrator.StopMonitoring(key);
            return Ok(new { message = $"Stopped monitoring {key}." });
        }
        /// <summary>
        /// Writes a command to a hardware device based on the provided key and payload. The payload contains the data to be written and the encoding format (e.g., HEX, UTF8, ASCII). The method first validates the payload and then converts the data into a byte array according to the specified encoding format. It then instructs the orchestrator to write the byte array to the device associated with the given key. If the write operation is successful, a confirmation message is returned; otherwise, an error message is provided, indicating that the command could not be dispatched.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost("Write/{key}")]
        [HttpOptions]
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
        /// <summary>
        /// Converts a hexadecimal string into a byte array. This helper method is used to process payloads that are provided in HEX format, allowing the controller to convert the string representation of the data into a format suitable for writing to hardware devices. The method removes any hyphens or spaces from the input string, then iterates through the cleaned string, converting each pair of hexadecimal characters into a byte and storing it in an array. This ensures that the data can be accurately transmitted to the hardware device in its expected binary format.
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        // Helper for Modbus/PLC hex strings
        private static byte[] ConvertFromHex(string hex)
        {
            hex = hex.Replace("-", "").Replace(" ", "");
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }
        /// <summary>
        /// Fetches a list of available hardware device keys from the configurations. This endpoint allows clients to retrieve all the keys that correspond to different hardware specifications defined in the application. By providing this list, clients can dynamically discover which hardware devices are supported and available for interaction, enabling them to make informed decisions about which devices to start monitoring or send commands to. The response is a simple list of strings representing the keys of the available hardware devices.
        /// </summary>
        /// <returns></returns>
        // =========================================================
        // NEW: Endpoint to fetch available device keys
        // =========================================================
        [HttpGet("Available")]
        public IActionResult GetAvailableDevices()
        {
            // Extract only the string keys (e.g., "Scanner_Mock", "Scanner1_TCP")
            List<string> devices = _configurations.HardwareSpecifications.Keys.ToList();
            return Ok(devices);
        }
    }
}