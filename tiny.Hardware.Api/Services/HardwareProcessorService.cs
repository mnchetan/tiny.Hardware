using System.Diagnostics;
using System.Text;
using tiny.Hardware.Core.Bus;
using tiny.Hardware.Core.Configurations;
using tiny.Hardware.Core.DataObjects;
using tiny.Hardware.Core.Extensions;
using tiny.WebApi.DataObjects;
namespace tiny.Hardware.Api.Services
{
    /// <summary>
    /// The HardwareProcessorService is a background service that continuously listens for hardware events published on the InternalHardwareBus. It processes incoming hardware data based on the configurations defined in ITinyHardwareConfigurations, allowing for dynamic plugin resolution and fallback logic. The service decodes raw byte data from hardware devices, applies any necessary post-processing through plugins, and handles errors gracefully while logging relevant information. This class serves as the core processing engine for handling hardware interactions within the tiny.Hardware framework, enabling flexible and extensible data processing based on the defined hardware specifications.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the HardwareProcessorService class with the specified hardware bus and configurations. This constructor sets up the necessary dependencies for the service to function, allowing it to consume hardware events from the bus and process them based on the provided configurations. The service relies on the InternalHardwareBus to receive hardware events and on ITinyHardwareConfigurations to determine how to handle and process those events according to the defined specifications.
    /// </remarks>
    /// <param name="bus"></param>
    /// <param name="configurations"></param>
    [DebuggerStepThrough]
    public class HardwareProcessorService(InternalHardwareBus bus, ITinyHardwareConfigurations configurations) : BackgroundService
    {
        /// <summary>
        /// The HardwareProcessorService class is responsible for consuming hardware events from the InternalHardwareBus and processing them according to the specifications defined in ITinyHardwareConfigurations. It uses dynamic plugin resolution to apply custom post-processing logic for different hardware types, and it includes fallback decoding logic based on configuration settings. The service runs continuously in the background, ensuring that all incoming hardware data is handled efficiently and effectively, while also providing robust error handling and logging capabilities.
        /// </summary>
        private readonly InternalHardwareBus _bus = bus;
        /// <summary>
        /// ITinyHardwareConfigurations provides access to the hardware specifications and configurations defined for the application. This allows the HardwareProcessorService to retrieve the necessary information about each hardware device based on a key, enabling dynamic handling of multiple hardware types without hardcoding specific logic for each one. The service uses this configuration to determine how to process incoming hardware data, including which plugins to apply and how to decode raw byte data when no plugin is available.
        /// </summary>
        private readonly ITinyHardwareConfigurations _configurations = configurations; // <-- CHANGED THIS

        /// <summary>
        /// The ExecuteAsync method is the main entry point for the background service, where it continuously listens for hardware events from the InternalHardwareBus. For each incoming event, it retrieves the corresponding hardware specification from ITinyHardwareConfigurations using the event's configuration key. The method then attempts to resolve and apply any post-processing plugins defined in the configuration. If no plugin is available, it falls back to a default decoding mechanism based on the specified encoding type. The processed data is then outputted, and any exceptions encountered during processing are logged appropriately. This method ensures that all hardware events are handled efficiently and according to the defined specifications, allowing for dynamic and flexible processing of hardware data.
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
                    // <-- CHANGED THE LOOKUP TO USE THE INTERFACE PROPERTY
                    if (!_configurations.HardwareSpecifications.TryGetValue(hwEvent.ConfigKey, out HardwareSpecification config) || config == null)
                    {
                        Global.LogError($"Received data for unknown configuration key: {hwEvent.ConfigKey}", null);
                        continue;
                    }
                    // 1. DYNAMIC PLUGIN RESOLUTION
                    IProcessHardwareData plugin = config.LoadPostProcessingPlugin();
                    dynamic finalResult;
                    if (plugin != null)
                    {
                        // 2. RUN THROUGH PLUGIN (e.g., WarehouseLogic.dll)
                        finalResult = plugin.PostProcessResponse(hwEvent.RawData);
                    }
                    else
                    {
                        // 3. CONFIG-DRIVEN FALLBACK LOGIC
                        finalResult = DecodeRawBytes(hwEvent.RawData, config.DefaultFallbackEncoding);
                    }
                    // 4. ACTION
                    Console.WriteLine($"[{hwEvent.ConfigKey}] PROCESSED DATA: {finalResult}");
                }
                catch (Exception ex)
                {
                    Global.LogError($"Failed to process hardware data for {hwEvent.ConfigKey}", ex);
                }
            }
        }
        /// <summary>
        /// DecodeRawBytes is a helper method that takes a byte array of raw data and an encoding type as input and returns a decoded string based on the specified encoding. It supports various encoding types such as UTF8, UTF16, HEX, and ASCII. If the raw data is null or empty, it returns an empty string. The method uses a switch expression to determine which encoding to apply based on the provided encoding type, allowing for flexible decoding of hardware data when no specific post-processing plugin is available.
        /// </summary>
        /// <param name="rawData"></param>
        /// <param name="encodingType"></param>
        /// <returns></returns>
        [DebuggerHidden]
        private static string DecodeRawBytes(byte[] rawData, string encodingType)
        {
            return rawData == null || rawData.Length == 0
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
}