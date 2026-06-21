using System.Diagnostics;
using System.Runtime.CompilerServices;
using tiny.Hardware.Core.DataObjects;
using tiny.WebApi.DataObjects;
namespace tiny.Hardware.Core.Providers
{
    /// <summary>
    /// The SerialHardwareProvider class is a mock implementation of the IHardwareProvider interface designed to simulate interactions with a serial device, such as a scale or sensor. This provider includes methods for connecting to the device, subscribing to data updates, and writing commands, all while demonstrating scalability and extensibility for future enhancements. The ConnectAsync method simulates establishing a connection to a serial device using the provided hardware specifications, while the SubscribeAsync method simulates receiving data from the device at regular intervals. The WriteAsync method allows for simulating command transmission to the device, providing a framework for future integration with actual serial communication logic. This mock implementation serves as a template for how a real serial provider could be structured within the tiny.Hardware framework, allowing for easy expansion and integration of real hardware communication in the future.
    /// </summary>
    // 3. SERIAL PORT IMPLEMENTATION (MOCK SHOWING SCALABILITY)
    [DebuggerStepThrough]
    public class SerialHardwareProvider : IHardwareProvider
    {
        /// <summary>
        /// Simulates establishing a connection to a serial device using the provided hardware specifications. In a real implementation, this method would utilize the details from the HardwareSpecification object, such as the IP address (which could represent a COM port in this context) and baud rate (represented by the Port property), to configure and open a serial connection to the device. The method currently logs the connection attempt for demonstration purposes and completes immediately, but it is structured to allow for future integration of actual serial communication logic, including error handling and connection management as needed for real hardware interactions.
        /// </summary>
        /// <param name="spec"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task ConnectAsync(HardwareSpecification spec, CancellationToken ct = default)
        {
            Global.LogDebug($"Opening COM Port {spec.IpAddress} at baud rate {spec.Port}");
            await Task.CompletedTask;
        }
        /// <summary>
        /// Simulates subscribing to data updates from the serial device. In a real implementation, this method would establish a continuous reading mechanism to receive data from the serial port, yielding byte arrays representing the incoming data for processing by the HardwareProcessorService. The method includes cancellation support to allow for graceful shutdown of the subscription when needed. For this mock implementation, it simulates receiving data by yielding a fixed byte array every 2 seconds until cancellation is requested, demonstrating where actual reading and data retrieval logic would be integrated in a future real-world implementation.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<byte[]> SubscribeAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    // This will throw when /Stop is called
                    await Task.Delay(2000, ct);
                }
                catch (OperationCanceledException)
                {
                    Global.LogDebug("Serial Subscription cancelled gracefully.");
                    break; // Exit the loop cleanly
                }

                // Safely yield outside the try-catch block
                yield return System.Text.Encoding.ASCII.GetBytes("SCALE_STABLE_10KG");
            }
        }
        /// <summary>
        /// Simulates writing a command to the serial device. In a real implementation, this method would convert the provided byte array payload into the appropriate serial command format and transmit it to the device using the established connection. The method returns a boolean indicating whether the write operation was successful, allowing calling code to handle any errors or issues that may arise during communication with the hardware. For this mock implementation, it simply logs the payload being "sent" to the device and returns true, demonstrating where actual command transmission logic would be integrated in a future real-world implementation.
        /// </summary>
        /// <returns></returns>
        public ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }
        /// <summary>
        /// Simulates writing a command to the serial device. In a real implementation, this method would convert the provided byte array payload into the appropriate serial command format and transmit it to the device using the established connection. The method returns a boolean indicating whether the write operation was successful, allowing calling code to handle any errors or issues that may arise during communication with the hardware. For this mock implementation, it simply logs the payload being "sent" to the device and returns true, demonstrating where actual command transmission logic would be integrated in a future real-world implementation.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<bool> WriteAsync(byte[] payload, CancellationToken ct = default)
        {
            // Simulate a successful write to a fake device
            string simulatedPayload = System.Text.Encoding.ASCII.GetString(payload);
            Global.LogDebug($"[MOCK TX] Successfully transmitted command to hardware: {simulatedPayload}");
            await Task.CompletedTask;
            return true;
        }
    }
}