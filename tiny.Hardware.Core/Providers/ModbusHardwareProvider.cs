using System.Diagnostics;
using System.Runtime.CompilerServices;
using tiny.Hardware.Core.DataObjects;
using tiny.WebApi.DataObjects;

namespace tiny.Hardware.Core.Providers
{
    // 2. MODBUS IMPLEMENTATION (MOCK SHOWING SCALABILITY)
    [DebuggerStepThrough]
    public class ModbusHardwareProvider : IHardwareProvider
    {
        public async Task ConnectAsync(HardwareSpecification spec, CancellationToken ct = default)
        {
            Global.LogDebug($"Connecting to Modbus PLC at {spec.IpAddress}");
            await Task.CompletedTask;
        }

        public async IAsyncEnumerable<byte[]> SubscribeAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    // This will throw when /Stop is called
                    await Task.Delay(5000, ct); // Poll registers every 5 seconds
                }
                catch (OperationCanceledException)
                {
                    Global.LogDebug("Modbus Subscription cancelled gracefully.");
                    break; // Exit the loop cleanly
                }

                // Safely yield outside the try-catch block
                yield return [0x00, 0x01, 0x00, 0x02]; // Mock register bytes
            }
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}