using System.Diagnostics;
using tiny.Hardware.Core.Providers;
using tiny.WebApi.DataObjects;

namespace tiny.Hardware.Core.Engine
{
    [DebuggerStepThrough]
    public static class HardwareProviderFactory
    {
        [DebuggerHidden]
        public static IHardwareProvider Create(string protocol)
        {
            Global.LogDebug($"Instantiating provider for protocol: {protocol}");
            return protocol.Trim().ToLowerInvariant() switch
            {
                "tcpip" => new TcpHardwareProvider(),
                "modbus" => new ModbusHardwareProvider(),
                "serial" => new SerialHardwareProvider(),
                _ => throw new NotSupportedException($"Protocol {protocol} is not supported.")
            };
        }
    }
}