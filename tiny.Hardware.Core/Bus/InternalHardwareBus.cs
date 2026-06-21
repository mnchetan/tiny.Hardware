using System.Diagnostics;
using System.Threading.Channels;
using tiny.WebApi.DataObjects;
namespace tiny.Hardware.Core.Bus
{
    /// <summary>
    /// The InternalHardwareBus class implements a simple in-memory message bus using System.Threading.Channels to facilitate communication between hardware monitoring components and the background processing service. It provides asynchronous methods for publishing hardware events and subscribing to them, allowing for efficient and thread-safe handling of hardware data as it flows through the system. This class serves as the central conduit for hardware events, enabling decoupled communication between producers (hardware monitors) and consumers (processing services) within the tiny.Hardware framework.
    /// </summary>
    [DebuggerStepThrough]
    public class InternalHardwareBus
    {
        /// <summary>
        /// The _channel field is a bounded channel that serves as the underlying data structure for the InternalHardwareBus. It is configured to hold a maximum of 10,000 hardware events, and it uses a wait strategy when the channel is full, ensuring that producers will block until space is available rather than losing events. This channel allows for efficient and thread-safe communication between producers and consumers of hardware events, enabling asynchronous publishing and subscribing while maintaining the integrity of the data flow within the tiny.Hardware framework.
        /// </summary>
        private readonly Channel<HardwareEvent> _channel;
        /// <summary>
        /// Initializes a new instance of the InternalHardwareBus class. This constructor sets up the underlying channel with a bounded capacity of 10,000 events and configures it to use a wait strategy when the channel is full. This ensures that producers will block rather than lose events when the channel reaches its capacity, allowing for reliable communication between hardware monitors and processing services within the tiny.Hardware framework. The constructor also logs a debug message indicating that the Internal Hardware Message Bus has been initialized with the specified settings.
        /// </summary>
        public InternalHardwareBus()
        {
            Global.LogDebug("Initializing Internal Hardware Message Bus (Bounded: 10000).");
            BoundedChannelOptions options = new(10000) { FullMode = BoundedChannelFullMode.Wait };
            _channel = Channel.CreateBounded<HardwareEvent>(options);
        }
        /// <summary>
        /// Publishes a hardware event to the bus asynchronously. This method takes a HardwareEvent object as input and writes it to the underlying channel. If the channel is full, the method will wait until space is available before writing the event, ensuring that no events are lost. This allows producers (such as hardware monitors) to reliably send hardware events to consumers (such as processing services) without worrying about capacity issues. The use of asynchronous programming ensures that the publishing process does not block the calling thread, allowing for efficient handling of hardware events in real-time.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public async ValueTask PublishAsync(HardwareEvent data) => await _channel.Writer.WriteAsync(data);
        /// <summary>
        /// Subscribes to the bus and returns an asynchronous stream of hardware events. This method allows consumers (such as processing services) to read hardware events from the underlying channel as they are published by producers (such as hardware monitors). The method takes a CancellationToken as a parameter, which can be used to cancel the subscription when it is no longer needed. The use of IAsyncEnumerable allows for efficient and responsive handling of hardware events, enabling consumers to process events in real-time as they arrive on the bus. This design promotes decoupled communication between producers and consumers within the tiny.Hardware framework, allowing for flexible and scalable event processing.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public IAsyncEnumerable<HardwareEvent> SubscribeAsync(CancellationToken ct) => _channel.Reader.ReadAllAsync(ct);
    }
}