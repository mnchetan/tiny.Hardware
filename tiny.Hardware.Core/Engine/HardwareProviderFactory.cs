using System.Diagnostics;
using tiny.Hardware.Core.Providers;
using tiny.WebApi.DataObjects;
namespace tiny.Hardware.Core.Engine
{
    /// <summary>
    /// The HardwareProviderFactory class is a static factory responsible for creating instances of IHardwareProvider based on the specified protocol. It uses a simple switch expression to determine which concrete implementation of IHardwareProvider to instantiate, allowing for easy extension in the future by adding new protocols and their corresponding providers. This factory abstracts away the instantiation logic for hardware providers, providing a centralized location for managing the creation of these objects and ensuring that the correct provider is used based on the protocol specified in the hardware configuration. By using this factory, the application can easily support multiple hardware communication protocols without needing to modify the core processing logic, promoting flexibility and maintainability within the tiny.Hardware framework.
    /// </summary>
    [DebuggerStepThrough]
    public static class HardwareProviderFactory
    {
        /// <summary>
        /// Creates an instance of IHardwareProvider based on the provided protocol string. The method supports "tcpip", "modbus", and "serial" protocols, returning the corresponding provider implementation for each. If an unsupported protocol is specified, a NotSupportedException is thrown to indicate that the requested protocol is not available within the factory's capabilities.
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
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