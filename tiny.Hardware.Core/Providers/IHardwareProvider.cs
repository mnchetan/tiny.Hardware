using tiny.Hardware.Core.DataObjects;

namespace tiny.Hardware.Core.Providers
{
    public interface IHardwareProvider : IAsyncDisposable
    {
        Task ConnectAsync(HardwareSpecification spec, CancellationToken ct = default);

        // This handles the continuous stream of data from the scanner
        IAsyncEnumerable<byte[]> SubscribeAsync(CancellationToken ct = default);
    }
}