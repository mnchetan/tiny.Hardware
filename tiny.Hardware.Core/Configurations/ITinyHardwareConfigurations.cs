using System.Diagnostics;
using tiny.Hardware.Core.DataObjects;
namespace tiny.Hardware.Core.Configurations
{
    /// <summary>
    /// The ITinyHardwareConfigurations interface defines a contract for accessing hardware specifications and configurations within the tiny.Hardware framework. It includes a property that holds a dictionary mapping configuration keys (strings) to HardwareSpecification objects, which contain the details about how to process data from different hardware devices. This interface allows for flexible and dynamic management of hardware configurations, enabling the application to support multiple hardware types without hardcoding specific logic for each one. Implementing this interface allows for easy retrieval of hardware specifications based on configuration keys, facilitating dynamic plugin resolution and processing of hardware data in the HardwareProcessorService.
    /// </summary>
    public interface ITinyHardwareConfigurations
    {
        /// <summary>
        /// HardwareSpecifications is a dictionary that maps configuration keys (strings) to HardwareSpecification objects. Each entry in the dictionary represents a specific hardware configuration, containing details such as the device type, encoding type, and any associated post-processing plugins. This property allows for dynamic retrieval of hardware specifications based on configuration keys, enabling the application to handle multiple hardware types flexibly and efficiently. The HardwareProcessorService uses this dictionary to determine how to process incoming hardware data according to the defined specifications for each device.
        /// </summary>
        [DebuggerHidden]
        Dictionary<string, HardwareSpecification> HardwareSpecifications { get; set; }
    }
}