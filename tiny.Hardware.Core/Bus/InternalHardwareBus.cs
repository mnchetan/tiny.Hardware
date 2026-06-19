using System.Diagnostics;
using System.Threading.Channels;
using tiny.WebApi.DataObjects;

namespace tiny.Hardware.Core.Bus
{
    [DebuggerStepThrough]
    public class InternalHardwareBus
    {
        private readonly Channel<HardwareEvent> _channel;

        public InternalHardwareBus()
        {
            Global.LogDebug("Initializing Internal Hardware Message Bus (Bounded: 10000).");
            BoundedChannelOptions options = new(10000) { FullMode = BoundedChannelFullMode.Wait };
            _channel = Channel.CreateBounded<HardwareEvent>(options);
        }

        [DebuggerHidden]
        public async ValueTask PublishAsync(HardwareEvent data) => await _channel.Writer.WriteAsync(data);

        [DebuggerHidden]
        public IAsyncEnumerable<HardwareEvent> SubscribeAsync(CancellationToken ct) => _channel.Reader.ReadAllAsync(ct);
    }
}