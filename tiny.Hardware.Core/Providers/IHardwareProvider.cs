using tiny.Hardware.Core.DataObjects;
namespace tiny.Hardware.Core.Providers
{
    /// <summary>
    /// The IHardwareProvider interface defines the contract for hardware providers within the tiny.Hardware framework. It includes methods for connecting to hardware devices based on a given specification, subscribing to data streams from the hardware, and writing data back to the hardware (bi-directional support). Implementations of this interface are responsible for handling the specific communication protocols and interactions with the hardware devices, allowing for flexible and extensible support for various types of hardware within the application. By adhering to this interface, different hardware providers can be easily integrated and managed within the HardwareProcessorService and other components that rely on hardware interactions.
    /// </summary>
    public interface IHardwareProvider : IAsyncDisposable
    {
        /// <summary>
        /// Connects to the hardware device based on the provided HardwareSpecification. This method is responsible for establishing the necessary communication channels and preparing the provider for data exchange with the hardware. The connection process may involve setting up network connections, initializing serial ports, or any other protocol-specific setup required by the hardware. The method accepts a CancellationToken to allow for graceful cancellation of the connection process if needed.
        /// </summary>
        /// <param name="spec"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task ConnectAsync(HardwareSpecification spec, CancellationToken ct = default);
        /// <summary>
        /// Subscribes to the data stream from the hardware device, returning an asynchronous enumerable of byte arrays. This method allows consumers to receive data from the hardware in a streaming fashion, enabling real-time processing of incoming data. The subscription can be cancelled using the provided CancellationToken, allowing for graceful shutdown of the data stream when necessary. Implementations of this method should handle the specific mechanics of subscribing to the hardware's data output based on the communication protocol and ensure that data is emitted as byte arrays for further processing by the HardwareProcessorService or other components.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        IAsyncEnumerable<byte[]> SubscribeAsync(CancellationToken ct = default);
        /// <summary>
        /// Writes data back to the hardware device. This method allows for bi-directional communication with the hardware, enabling the application to send commands or data to the device in response to incoming data or based on user interactions. The method accepts a byte array as the payload to be written to the hardware and a CancellationToken for graceful cancellation of the write operation if needed. Implementations of this method should handle the specific mechanics of writing data to the hardware based on the communication protocol, ensuring that the data is transmitted correctly and efficiently to the device. The method returns a boolean indicating whether the write operation was successful, allowing calling code to handle any errors or issues that may arise during communication with the hardware.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        // NEW: Bi-directional support
        Task<bool> WriteAsync(byte[] payload, CancellationToken ct = default);
    }
}