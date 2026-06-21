using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using tiny.Hardware.Core.DataObjects;
using tiny.WebApi.DataObjects;
namespace tiny.Hardware.Core.Providers
{
    /// <summary>
    /// The TcpHardwareProvider class is a concrete implementation of the IHardwareProvider interface designed to facilitate communication with hardware devices over TCP/IP. This provider establishes a TCP connection to the specified IP address and port, allowing for bidirectional communication with the hardware. The ConnectAsync method handles the connection setup, while the SubscribeAsync method continuously listens for incoming data from the hardware, yielding byte arrays for processing. The WriteAsync method enables sending commands or data back to the hardware device. This implementation includes robust error handling and cancellation support to ensure graceful shutdowns and reliable communication, making it suitable for real-world applications where stable and efficient TCP communication is required within the tiny.Hardware framework.
    /// </summary>
    [DebuggerStepThrough]
    public class TcpHardwareProvider : IHardwareProvider
    {
        /// <summary>
        /// The TcpClient instance used to manage the TCP connection to the hardware device. This field is initialized in the ConnectAsync method when a connection is established, and it is used for sending and receiving data over the network. The NetworkStream field is derived from the TcpClient and is used for reading and writing data to the hardware device. Both fields are disposed of in the DisposeAsync method to ensure proper cleanup of resources when the provider is no longer needed.
        /// </summary>
        private TcpClient _client;
        /// <summary>
        /// The NetworkStream instance used for reading and writing data to the hardware device over the established TCP connection. This stream is obtained from the TcpClient after a successful connection is made in the ConnectAsync method. It is used in the SubscribeAsync method to read incoming data from the hardware and in the WriteAsync method to send data back to the device. The stream is properly disposed of in the DisposeAsync method to ensure that all network resources are released when the provider is no longer needed.
        /// </summary>
        private NetworkStream _stream;
        /// <summary>
        /// Asynchronously establishes a TCP connection to the hardware device specified in the HardwareSpecification object. This method initializes the TcpClient and connects to the IP address and port defined in the specification. Upon successful connection, it retrieves the NetworkStream for subsequent data communication. The method supports cancellation through a CancellationToken, allowing for graceful shutdowns if needed. Proper error handling is implemented to ensure that any connection issues are logged and can be addressed accordingly.
        /// </summary>
        /// <param name="spec"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task ConnectAsync(HardwareSpecification spec, CancellationToken ct = default)
        {
            Global.LogDebug($"Connecting TCP Socket to {spec.IpAddress}:{spec.Port}");
            _client = new TcpClient();
            await _client.ConnectAsync(spec.IpAddress, spec.Port, ct);
            _stream = _client.GetStream();
        }
        /// <summary>
        /// Asynchronously subscribes to the data stream from the hardware device over the established TCP connection. This method continuously listens for incoming data on the NetworkStream, yielding byte arrays as they are received. It includes proactive checks to detect if the remote host has closed the connection and handles cancellation gracefully to allow for clean shutdowns. The method ensures that data is yielded outside of the try-catch block to prevent issues with yielding during exception handling, providing a robust and efficient mechanism for receiving data from the hardware device in real-time.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        public async IAsyncEnumerable<byte[]> SubscribeAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            byte[] buffer = new byte[4096];

            while (!ct.IsCancellationRequested)
            {
                byte[] payloadToYield = null;
                try
                {
                    // 1. Proactive check to see if the scanner dropped offline
                    if (_client.Client.Poll(0, SelectMode.SelectRead) && _client.Client.Available == 0)
                        throw new IOException("Remote host closed connection.");
                    // 2. If no data, yield the thread. 
                    // This throws TaskCanceledException when you call /Stop!
                    if (!_stream.DataAvailable)
                    {
                        await Task.Delay(10, ct);
                        continue;
                    }
                    // 3. Read the data
                    int bytesRead = await _stream.ReadAsync(buffer, ct);
                    if (bytesRead > 0)
                    {
                        payloadToYield = new byte[bytesRead];
                        Array.Copy(buffer, payloadToYield, bytesRead);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Catch the cancellation gracefully
                    Global.LogDebug("TCP Subscription cancelled gracefully.");
                    break;
                }
                catch (Exception)
                {
                    // Let other real socket exceptions bubble up
                    throw;
                }
                // 4. Safely yield the data OUTSIDE the try-catch block!
                if (payloadToYield != null)
                {
                    yield return payloadToYield;
                }
            }
        }
        /// <summary>
        /// Asynchronously writes a byte array payload back to the hardware device over the established TCP connection. This method checks if the connection is still open before attempting to write, and it handles any exceptions that may occur during the write operation, logging errors as needed. The method returns a boolean indicating whether the write operation was successful, allowing calling code to handle any issues that may arise during communication with the hardware. This implementation ensures that data is transmitted correctly and efficiently to the device while providing robust error handling for reliable communication.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        // Add this method inside TcpHardwareProvider
        public async Task<bool> WriteAsync(byte[] payload, CancellationToken ct = default)
        {
            if (_client == null || !_client.Connected || _stream == null)
            {
                Global.LogError("Cannot write to TCP socket. Connection is not open.", null);
                return false;
            }
            try
            {
                await _stream.WriteAsync(payload, ct);
                Global.LogDebug($"Successfully wrote {payload.Length} bytes to TCP socket.");
                return true;
            }
            catch (Exception ex)
            {
                Global.LogError("Failed to write to TCP socket.", ex);
                return false;
            }
        }
        /// <summary>
        /// Asynchronously disposes of the TcpHardwareProvider, ensuring that all network resources are properly released. This method checks if the NetworkStream and TcpClient are not null before attempting to dispose of them, and it suppresses finalization to optimize garbage collection. The method is designed to be called when the provider is no longer needed, allowing for graceful cleanup of resources and preventing potential memory leaks or lingering network connections. By implementing IAsyncDisposable, this class can be used with asynchronous disposal patterns, ensuring that all resources are released efficiently in an asynchronous context.
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            _stream?.Dispose();
            _client?.Dispose();
            await Task.CompletedTask;
        }
    }
}