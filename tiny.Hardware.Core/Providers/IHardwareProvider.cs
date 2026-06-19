using tiny.Hardware.Core.DataObjects;

namespace tiny.Hardware.Core.Providers
{
    public interface IHardwareProvider : IAsyncDisposable
    {
        Task ConnectAsync(HardwareSpecification spec, CancellationToken ct = default);
        IAsyncEnumerable<byte[]> SubscribeAsync(CancellationToken ct = default);

        // NEW: Bi-directional support
        Task<bool> WriteAsync(byte[] payload, CancellationToken ct = default);
    }
}