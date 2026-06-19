using System.Diagnostics;
using System.Text;
using tiny.Hardware.Core.Bus;
using tiny.Hardware.Core.Configurations; // Add this using statement!
using tiny.Hardware.Core.DataObjects;
using tiny.Hardware.Core.Extensions;
using tiny.WebApi.DataObjects; // Global.cs

namespace tiny.Hardware.Api.Services
{
    [DebuggerStepThrough]
    public class HardwareProcessorService : BackgroundService
    {
        private readonly InternalHardwareBus _bus;
        private readonly ITinyHardwareConfigurations _configurations; // <-- CHANGED THIS

        // <-- CHANGED THE CONSTRUCTOR INJECTION
        public HardwareProcessorService(InternalHardwareBus bus, ITinyHardwareConfigurations configurations)
        {
            _bus = bus;
            _configurations = configurations;
        }

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

        [DebuggerHidden]
        private string DecodeRawBytes(byte[] rawData, string encodingType)
        {
            return rawData == null || rawData.Length == 0
                ? string.Empty
                : encodingType?.Trim().ToUpperInvariant() switch
                {
                    "UTF8" => Encoding.UTF8.GetString(rawData).Trim(),
                    "UTF16" => Encoding.Unicode.GetString(rawData).Trim(),
                    "HEX" => BitConverter.ToString(rawData).Replace("-", ""),
                    "ASCII" => Encoding.ASCII.GetString(rawData).Trim(),
                    _ => Encoding.ASCII.GetString(rawData).Trim()
                };
        }
    }
}