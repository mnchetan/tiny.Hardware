using System.Collections.Concurrent;
using System.Diagnostics;
using tiny.Hardware.Core.Bus;
using tiny.Hardware.Core.DataObjects;
using tiny.Hardware.Core.Providers;
using tiny.WebApi.DataObjects;

namespace tiny.Hardware.Core.Engine
{
    [DebuggerStepThrough]
    public class HardwareOrchestrator
    {
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _activeMonitors = new();
        private readonly InternalHardwareBus _bus;

        public HardwareOrchestrator(InternalHardwareBus bus)
        {
            _bus = bus;
        }

        public async Task StartMonitoringAsync(string configKey, HardwareSpecification config)
        {
            Global.LogDebug($"Starting monitoring job for Key: {configKey}");
            if (_activeMonitors.ContainsKey(configKey))
                throw new InvalidOperationException($"Hardware job {configKey} is already running.");

            CancellationTokenSource cts = new();
            _activeMonitors.TryAdd(configKey, cts);

            _ = Task.Run(() => MonitorDeviceProcess(configKey, config, cts.Token), cts.Token);
        }

        public void StopMonitoring(string configKey)
        {
            Global.LogDebug($"Stopping monitoring job for Key: {configKey}");
            if (_activeMonitors.TryRemove(configKey, out CancellationTokenSource? cts))
            {
                cts.Cancel();
                cts.Dispose();
            }
        }

        private async Task MonitorDeviceProcess(string configKey, HardwareSpecification config, CancellationToken ct)
        {
            await using IHardwareProvider provider = HardwareProviderFactory.Create(config.Protocol);
            try
            {
                await provider.ConnectAsync(config, ct);
                await foreach (var rawBytes in provider.SubscribeAsync(ct))
                {
                    // Push to the internal messaging queue instantly
                    await _bus.PublishAsync(new HardwareEvent
                    {
                        ConfigKey = configKey,
                        RawData = rawBytes
                    });
                }
            }
            catch (OperationCanceledException) { Global.LogDebug($"Job {configKey} cancelled successfully."); }
            catch (Exception ex)
            {
                Global.LogError($"Device for {configKey} disconnected unexpectedly.", ex);
            }
        }
    }
}