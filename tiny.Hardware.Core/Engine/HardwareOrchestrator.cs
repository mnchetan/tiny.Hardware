using System.Collections.Concurrent;
using System.Diagnostics;
using tiny.Hardware.Core.Bus;
using tiny.Hardware.Core.DataObjects;
using tiny.Hardware.Core.Providers;
using tiny.WebApi.DataObjects;
namespace tiny.Hardware.Core.Engine
{
    /// <summary>
    /// The HardwareOrchestrator class is responsible for managing the lifecycle of hardware interactions, including starting and stopping monitoring sessions and writing commands to devices. It abstracts away the complexities of handling different hardware types and provides a unified interface for the controller to interact with. The orchestrator maintains a concurrent dictionary to track active monitoring sessions for each hardware configuration key, allowing for efficient management of multiple devices simultaneously. It also handles the connection and subscription logic for each device, ensuring that hardware events are published to the InternalHardwareBus for processing by the HardwareProcessorService. Additionally, it provides a method for writing commands to active devices, enabling dynamic interaction with hardware based on the defined specifications.
    /// </summary>
    /// <param name="bus"></param>
    [DebuggerStepThrough]
    public class HardwareOrchestrator(InternalHardwareBus bus)
    {
        /// <summary>
        /// The ActiveDeviceState class is an internal wrapper used to hold the state of an active hardware monitoring session. It contains a reference to the IHardwareProvider instance that is responsible for managing the connection and subscription to the hardware device, as well as a CancellationTokenSource that allows for graceful cancellation of the monitoring process when needed. This class is used within the HardwareOrchestrator to track and manage the state of each active device based on its configuration key, enabling efficient handling of multiple devices simultaneously while ensuring proper resource management and cleanup when monitoring sessions are stopped or cancelled.
        /// </summary>
        // NEW: Internal wrapper to hold state
        private class ActiveDeviceState
        {
            /// <summary>
            /// The Provider property holds a reference to the IHardwareProvider instance that is responsible for managing the connection and subscription to the hardware device. This provider is created based on the protocol specified in the hardware configuration and is used to interact with the device, including connecting, subscribing to data streams, and writing commands. The provider encapsulates the logic for communicating with the specific hardware type, allowing the HardwareOrchestrator to abstract away these details and provide a unified interface for managing different devices.
            /// </summary>
            public IHardwareProvider Provider { get; set; }
            /// <summary>
            /// The Cts property is a CancellationTokenSource that allows for graceful cancellation of the monitoring process for a specific hardware device. It is used to signal when a monitoring session should be stopped, either due to an explicit stop request from the controller or when an error occurs during the monitoring process. By using a CancellationTokenSource, the HardwareOrchestrator can ensure that all resources associated with the monitoring session are properly cleaned up and that any ongoing operations are safely terminated when cancellation is requested.
            /// </summary>
            public CancellationTokenSource Cts { get; set; }
        }
        /// <summary>
        /// The _activeDevices field is a concurrent dictionary that maps hardware configuration keys to their corresponding ActiveDeviceState instances. This dictionary is used to track the state of each active monitoring session for different hardware devices, allowing the HardwareOrchestrator to efficiently manage multiple devices simultaneously. The use of a concurrent dictionary ensures thread-safe access and modifications to the collection, enabling the orchestrator to handle start and stop requests for devices without running into
        /// </summary>
        private readonly ConcurrentDictionary<string, ActiveDeviceState> _activeDevices = new();
        /// <summary>
        /// The _bus field is a reference to the InternalHardwareBus instance that serves as the central conduit for hardware events within the tiny.Hardware framework. The HardwareOrchestrator uses this bus to publish hardware events received from active monitoring sessions, allowing the HardwareProcessorService to consume and process these events in real-time. By leveraging the InternalHardwareBus, the orchestrator can decouple the production of hardware events from their consumption, enabling a flexible and scalable architecture for handling hardware interactions across different devices and configurations.
        /// </summary>
        private readonly InternalHardwareBus _bus = bus;
        /// <summary>
        /// The StartMonitoringAsync method is responsible for initiating a monitoring session for a specific hardware device based on the provided configuration key and hardware specification. It first checks if a monitoring session for the given configuration key is already active, throwing an exception if it is. If not, it creates a new ActiveDeviceState instance, which includes an IHardwareProvider created based on the protocol specified in the hardware configuration and a CancellationTokenSource for managing the lifecycle of the monitoring session. The method then adds this state to the _activeDevices dictionary and starts a new task to run the MonitorDeviceProcess method, which handles the connection and subscription logic for the device. This allows the orchestrator to manage multiple devices concurrently while ensuring that each device's monitoring session is properly tracked and can be cancelled when needed.
        /// </summary>
        /// <param name="configKey"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
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
        /// <summary>
        /// The WriteToDeviceAsync method is responsible for sending a command or data payload to an active hardware device based on the provided configuration key. It first checks if there is an active monitoring session for the given configuration key in the _activeDevices dictionary. If a session is found, it retrieves the corresponding ActiveDeviceState and uses its IHardwareProvider instance to write the payload to the device asynchronously. If no active session is found for the specified configuration key, the method logs an error message indicating that the device is not currently active and returns false. This method allows for dynamic interaction with hardware devices while they are being monitored, enabling real-time control and communication based on the defined specifications.
        /// </summary>
        /// <param name="configKey"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        // NEW: The Write Method
        public async Task<bool> WriteToDeviceAsync(string configKey, byte[] payload)
        {
            if (_activeDevices.TryGetValue(configKey, out ActiveDeviceState state))
            {
                Global.LogDebug($"Dispatching write command to {configKey}.");
                return await state.Provider.WriteAsync(payload);
            }
            Global.LogError($"Cannot write. Device {configKey} is not currently active.", null);
            return false;
        }
        /// <summary>
        /// The StopMonitoring method is responsible for terminating an active monitoring session for a specific hardware device based on the provided configuration key. It checks if there is an active session for the given configuration key in the _activeDevices dictionary. If a session is found, it retrieves the corresponding ActiveDeviceState, cancels the monitoring process using the CancellationTokenSource, and disposes of the token source to free up resources. The actual disposal of the IHardwareProvider instance happens safely within the MonitorDeviceProcess method's finally block, ensuring that all resources are properly cleaned up when a monitoring session is stopped. This method allows for graceful termination of hardware monitoring sessions, ensuring that any ongoing operations are safely cancelled and that resources are released appropriately.
        /// </summary>
        /// <param name="configKey"></param>
        public void StopMonitoring(string configKey)
        {
            Global.LogDebug($"Stopping monitoring job for Key: {configKey}");
            if (_activeDevices.TryRemove(configKey, out ActiveDeviceState state))
            {
                state.Cts.Cancel();
                state.Cts.Dispose();
                // Note: The DisposeAsync on the provider happens safely inside the Monitor loop's finally block
            }
        }
        /// <summary>
        /// The MonitorDeviceProcess method is an internal asynchronous method responsible for managing the connection and subscription logic for a specific hardware device based on its configuration. It attempts to connect to the device using the IHardwareProvider instance and then subscribes to the data stream from the device. As raw byte data is received from the device, it publishes hardware events to the InternalHardwareBus, allowing other components of the system to process these events in real-time. The method also includes error handling to catch any exceptions that may occur during the monitoring process, logging appropriate messages for cancellation or unexpected disconnections. Finally, it ensures that resources are properly cleaned up by disposing of the IHardwareProvider instance when the monitoring session ends, whether due to cancellation or an error.
        /// </summary>
        /// <param name="configKey"></param>
        /// <param name="config"></param>
        /// <param name="state"></param>
        /// <returns></returns>
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