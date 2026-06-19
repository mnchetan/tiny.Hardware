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
        private TcpClient _client;
        private NetworkStream _stream;

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

        public async ValueTask DisposeAsync()
        {
            _stream?.Dispose();
            _client?.Dispose();
            await Task.CompletedTask;
        }
    }
}