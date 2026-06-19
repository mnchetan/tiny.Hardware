using System.Diagnostics;
using System.Runtime.CompilerServices;
using tiny.Hardware.Core.DataObjects;
using tiny.WebApi.DataObjects;

namespace tiny.Hardware.Core.Providers
{
    // 3. SERIAL PORT IMPLEMENTATION (MOCK SHOWING SCALABILITY)
    [DebuggerStepThrough]
    public class SerialHardwareProvider : IHardwareProvider
    {
        public async Task ConnectAsync(HardwareSpecification spec, CancellationToken ct = default)
        {
            Global.LogDebug($"Opening COM Port {spec.IpAddress} at baud rate {spec.Port}");
            await Task.CompletedTask;
        }

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

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}