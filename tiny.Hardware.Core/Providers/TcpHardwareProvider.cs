using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using tiny.Hardware.Core.DataObjects;
using tiny.WebApi.DataObjects; // Global.cs

namespace tiny.Hardware.Core.Providers
{
    [DebuggerStepThrough]
    public class TcpHardwareProvider : IHardwareProvider
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private TcpClient _client;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private NetworkStream _stream;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public async Task ConnectAsync(HardwareSpecification spec, CancellationToken ct = default)
        {
            Global.LogDebug($"Connecting TCP Socket to {spec.IpAddress}:{spec.Port}");
            _client = new TcpClient();
            await _client.ConnectAsync(spec.IpAddress, spec.Port, ct);
            _stream = _client.GetStream();
        }

        public async IAsyncEnumerable<byte[]> SubscribeAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            byte[] buffer = new byte[4096];

            while (!ct.IsCancellationRequested)
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                byte[] payloadToYield = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

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

        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            _stream?.Dispose();
            _client?.Dispose();
            await Task.CompletedTask;
        }
    }
}