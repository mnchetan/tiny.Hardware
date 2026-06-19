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

        public ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        // Add this method inside SerialHardwareProvider
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