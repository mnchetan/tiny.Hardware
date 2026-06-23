using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using tiny.Hardware.Core.DataObjects;
using tiny.WebApi.DataObjects;

namespace tiny.Hardware.Core.Providers
{
    /// <summary>
    /// Mock implementation of the IHardwareProvider interface for testing and development purposes. This class simulates a hardware device by providing a virtual connection, generating fake scan data, and handling command acknowledgments without requiring actual hardware. It is designed to facilitate the development and testing of applications that interact with hardware devices by mimicking their behavior in a controlled environment.
    /// </summary>
    [DebuggerStepThrough]
    public class MockHardwareProvider : IHardwareProvider
    {
        /// <summary>
        /// A thread-safe queue that holds acknowledgment messages to be sent back to the orchestrator stream. This queue acts as the internal reply buffer for the mock hardware, allowing it to simulate bi-directional communication by storing responses to commands received from the orchestrator. When a command is written to the mock hardware, an acknowledgment message is generated and enqueued here, which can then be retrieved and yielded during the subscription process.
        /// </summary>
        // Acts as the hardware's internal reply buffer for bi-directional communication
        private readonly ConcurrentQueue<byte[]> _deviceReplyBuffer = new();
        /// <summary>
        /// Simulates connecting to a virtual hardware device based on the provided HardwareSpecification. In this mock implementation, it logs the connection attempt and completes immediately without establishing a real network connection. This method is useful for testing and development scenarios where actual hardware is not available, allowing developers to verify the behavior of their applications when interacting with a hardware provider.
        /// </summary>
        /// <param name="spec"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public Task ConnectAsync(HardwareSpecification spec, CancellationToken ct = default)
        {
            // FIX 1: Removed spec.DeviceName and replaced with IpAddress and Port
            Global.LogDebug($"[MOCK] Connecting to virtual device at {spec.IpAddress}:{spec.Port}...");
            return Task.CompletedTask;
        }
        /// <summary>
        /// Subscribes to a simulated hardware data stream. This method continuously yields byte arrays representing either acknowledgment messages for commands sent to the mock hardware or simulated scan data at regular intervals. The subscription can be cancelled via the provided CancellationToken, allowing for graceful termination of the data stream. This implementation is useful for testing and development purposes, as it mimics the behavior of a real hardware device without requiring actual hardware.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<byte[]> SubscribeAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            int counter = 1;
            while (!ct.IsCancellationRequested)
            {
                byte[] payloadToYield = null;
                try
                {
                    // 1. PRIORITY READ: Check if the device needs to send an ACK for a command it just received
                    if (_deviceReplyBuffer.TryDequeue(out byte[] ackPayload))
                    {
                        payloadToYield = ackPayload;
                        // Small delay to simulate hardware processing speed before resuming normal scans
                        await Task.Delay(200, ct);
                    }
                    else
                    {
                        // 2. NORMAL READ: Emit a fake hardware scan every 3 seconds
                        await Task.Delay(3000, ct);
                        string payload = $"BOX_ID_{counter:D4}|WEIGHT_15.5KG\r\n";
                        payloadToYield = Encoding.ASCII.GetBytes(payload);
                        counter++;
                    }
                }
                catch (OperationCanceledException)
                {
                    Global.LogDebug("Mock Subscription cancelled gracefully.");
                    break;
                }
                // FIX 2: Safely yield outside the try-catch block!
                if (payloadToYield != null)
                {
                    yield return payloadToYield;
                }
            }
        }
        /// <summary>
        /// Simulates writing a command to the hardware device. In this mock implementation, it logs the received command and queues up an acknowledgment reply to be sent back to the orchestrator stream. This allows for testing the full communication flow without requiring actual hardware.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<bool> WriteAsync(byte[] payload, CancellationToken ct = default)
        {
            // The API calls this. We simulate the hardware receiving it.
            string receivedCommand = Encoding.ASCII.GetString(payload).Trim();
            Global.LogDebug($"[MOCK TX] Hardware Received Command: {receivedCommand}");
            // Queue up an acknowledgment reply to instantly push back to the orchestrator stream
            string ackReply = $"ACK_COMMAND_RECEIVED: [{receivedCommand}]";
            _deviceReplyBuffer.Enqueue(Encoding.ASCII.GetBytes(ackReply));
            await Task.CompletedTask;
            return true;
        }
        /// <summary>
        /// Disposes of any resources used by the mock hardware provider. In this implementation, there are no unmanaged resources to clean up, so the method simply returns a completed task. This is included to satisfy the IAsyncDisposable interface and maintain consistency with other hardware providers that may require cleanup.
        /// </summary>
        /// <returns></returns>
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}