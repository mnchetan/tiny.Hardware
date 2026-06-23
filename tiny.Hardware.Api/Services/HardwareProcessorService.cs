using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Text;
using tiny.Hardware.Api.Hubs;
using tiny.Hardware.Core.Bus;
using tiny.Hardware.Core.Configurations;
using tiny.Hardware.Core.DataObjects;
using tiny.Hardware.Core.Extensions;
using tiny.WebApi.DataObjects;

namespace tiny.Hardware.Api.Services
{
    /// <summary>
    /// Background service that listens to the internal hardware bus, processes incoming hardware events, and broadcasts the processed data to connected Angular clients via SignalR.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="HardwareProcessorService"/> class with the specified dependencies.
    /// </remarks>
    /// <param name="bus"></param>
    /// <param name="configurations"></param>
    /// <param name="hubContext"></param>
    [DebuggerStepThrough]
    public class HardwareProcessorService(
        InternalHardwareBus bus,
        ITinyHardwareConfigurations configurations,
        IHubContext<HardwareHub> hubContext) : BackgroundService
    {
        /// <summary>
        /// The internal hardware bus that provides a stream of hardware events to be processed.
        /// </summary>
        private readonly InternalHardwareBus _bus = bus;
        /// <summary>
        /// The configuration service that provides hardware specifications and settings for processing hardware events.
        /// </summary>
        private readonly ITinyHardwareConfigurations _configurations = configurations;
        /// <summary>
        /// The SignalR hub context used to broadcast processed hardware data to all connected Angular clients.
        /// </summary>
        // NEW: The SignalR Context that talks to Angular
        private readonly IHubContext<HardwareHub> _hubContext = hubContext;

        /// <summary>
        /// Executes the background service, continuously consuming hardware events from the internal bus, processing them according to their specifications, and broadcasting the results to connected Angular clients via SignalR.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Global.LogDebug("Hardware Processor Background Service started consuming bus.");

            await foreach (HardwareEvent hwEvent in _bus.SubscribeAsync(stoppingToken))
            {
                try
                {
                    if (!_configurations.HardwareSpecifications.TryGetValue(hwEvent.ConfigKey, out HardwareSpecification config) || config == null)
                    {
                        continue;
                    }
                    IProcessHardwareData plugin = config.LoadPostProcessingPlugin();
                    dynamic finalResult = plugin != null ? plugin.PostProcessResponse(hwEvent.RawData) : DecodeRawBytes(hwEvent.RawData, config.DefaultFallbackEncoding);
                    string resultString = finalResult.ToString();
                    Console.WriteLine($"[{hwEvent.ConfigKey}] PROCESSED DATA: {resultString}");

                    // NEW: BROADCAST DIRECTLY TO ALL CONNECTED ANGULAR UIs
                    await _hubContext.Clients.All.SendAsync("ReceiveHardwareUpdate", hwEvent.ConfigKey, resultString, stoppingToken);
                }
                catch (Exception ex)
                {
                    Global.LogError($"Failed to process hardware data for {hwEvent.ConfigKey}", ex);
                }
            }
        }
        /// <summary>
        /// Decodes the raw byte array into a string based on the specified encoding type. If the encoding type is not recognized, it defaults to ASCII encoding.
        /// </summary>
        /// <param name="rawData"></param>
        /// <param name="encodingType"></param>
        /// <returns></returns>
        [DebuggerHidden]
        private static string DecodeRawBytes(byte[] rawData, string encodingType) => rawData == null || rawData.Length == 0
                ? string.Empty
                : encodingType?.Trim().ToUpperInvariant() switch
                {
                    "UTF8" => Encoding.UTF8.GetString(rawData).Trim(),
                    "UTF16" => Encoding.Unicode.GetString(rawData).Trim(),
                    "HEX" => Convert.ToHexString(rawData),
                    "ASCII" => Encoding.ASCII.GetString(rawData).Trim(),
                    _ => Encoding.ASCII.GetString(rawData).Trim()
                };
    }
}