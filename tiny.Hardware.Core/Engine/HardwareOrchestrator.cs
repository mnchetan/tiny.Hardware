using System.Collections.Concurrent;
using System.Diagnostics;
using tiny.Hardware.Core.Bus;
using tiny.Hardware.Core.DataObjects;
using tiny.Hardware.Core.Providers;
using tiny.WebApi.DataObjects;

namespace tiny.Hardware.Core.Engine
{
    [DebuggerStepThrough]
    public class HardwareOrchestrator(InternalHardwareBus bus)
    {
        // NEW: Internal wrapper to hold state
        private class ActiveDeviceState
        {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
            public IHardwareProvider Provider { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
            public CancellationTokenSource Cts { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        }

        private readonly ConcurrentDictionary<string, ActiveDeviceState> _activeDevices = new();
        private readonly InternalHardwareBus _bus = bus;

        public async Task StartMonitoringAsync(string configKey, HardwareSpecification config)
        {
            Global.LogDebug($"Starting monitoring job for Key: {configKey}");
            if (_activeDevices.ContainsKey(configKey))
                throw new InvalidOperationException($"Hardware job {configKey} is already running.");

            ActiveDeviceState state = new()
            {
                Provider = HardwareProviderFactory.Create(config.Protocol),
                Cts = new CancellationTokenSource()
            };

            _activeDevices.TryAdd(configKey, state);

            _ = Task.Run(() => MonitorDeviceProcess(configKey, config, state), state.Cts.Token);
        }

        // NEW: The Write Method
        public async Task<bool> WriteToDeviceAsync(string configKey, byte[] payload)
        {
            if (_activeDevices.TryGetValue(configKey, out ActiveDeviceState? state))
            {
                Global.LogDebug($"Dispatching write command to {configKey}.");
                return await state.Provider.WriteAsync(payload);
            }

            Global.LogError($"Cannot write. Device {configKey} is not currently active.", null);
            return false;
        }

        public void StopMonitoring(string configKey)
        {
            Global.LogDebug($"Stopping monitoring job for Key: {configKey}");
            if (_activeDevices.TryRemove(configKey, out ActiveDeviceState? state))
            {
                state.Cts.Cancel();
                state.Cts.Dispose();
                // Note: The DisposeAsync on the provider happens safely inside the Monitor loop's finally block
            }
        }

        private async Task MonitorDeviceProcess(string configKey, HardwareSpecification config, ActiveDeviceState state)
        {
            try
            {
                await state.Provider.ConnectAsync(config, state.Cts.Token);
                await foreach (byte[] rawBytes in state.Provider.SubscribeAsync(state.Cts.Token))
                {
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
            finally
            {
                await state.Provider.DisposeAsync();
            }
        }
    }
}