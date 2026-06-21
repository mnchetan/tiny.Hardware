namespace tiny.Hardware.Core.Extensions
{
    /// <summary>
    /// The IProcessHardwareData interface defines a contract for processing raw hardware data. It includes a single method, PostProcessResponse, which takes a byte array as input and returns a dynamic object. This interface is intended to be implemented by vendors or developers who want to provide custom logic for parsing and interpreting raw hardware responses from various devices. By implementing this interface, developers can create plugins that can be dynamically loaded and applied to specific hardware types based on the configurations defined in ITinyHardwareConfigurations. This allows for flexible and extensible processing of hardware data without the need for hardcoding specific logic for each device type within the core processing service.
    /// </summary>
    // The Interface vendors will implement
    public interface IProcessHardwareData
    {
        /// <summary>
        /// PostProcessResponse takes a byte array representing the raw response from a hardware device and processes it to extract relevant information. The method is designed to be implemented with custom logic specific to the type of hardware being processed, allowing for flexible parsing of different data formats. The output is a dynamic object that can contain any structured data derived from the raw response, such as device type, extracted values, status information, and timestamps. This method enables the creation of plugins that can be applied to specific hardware types based on the configurations defined in ITinyHardwareConfigurations, facilitating extensible and adaptable processing of hardware data within the tiny.Hardware framework.
        /// </summary>
        /// <param name="rawResponse"></param>
        /// <returns></returns>
        dynamic PostProcessResponse(byte[] rawResponse);
    }
}
