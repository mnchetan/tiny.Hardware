using System.Diagnostics;
using tiny.Hardware.Core.DataObjects;
namespace tiny.Hardware.Core.Configurations
{
    /// <summary>
    /// The TinyHardwareConfigurations class is a concrete implementation of the ITinyHardwareConfigurations interface, providing a dictionary to store hardware specifications and configurations. This class serves as the central repository for all hardware-related configurations within the tiny.Hardware framework, allowing for easy access and management of different hardware specifications based on configuration keys. By implementing the ITinyHardwareConfigurations interface, this class enables dynamic retrieval of hardware specifications, facilitating flexible and efficient processing of hardware data in the HardwareProcessorService and other components that rely on these configurations.
    /// </summary>
    [DebuggerStepThrough]
    public class TinyHardwareConfigurations : ITinyHardwareConfigurations
    {
        /// <summary>
        /// HardwareSpecifications is a dictionary that maps configuration keys (strings) to HardwareSpecification objects. Each entry in the dictionary represents a specific hardware configuration, containing details such as the device type, encoding type, and any associated post-processing plugins. This property allows for dynamic retrieval of hardware specifications based on configuration keys, enabling the application to handle multiple hardware types flexibly and efficiently. The HardwareProcessorService uses this dictionary to determine how to process incoming hardware data according to the defined specifications for each device.
        /// </summary>
        [DebuggerHidden]
        public Dictionary<string, HardwareSpecification> HardwareSpecifications { get; set; } = [];
    }
}
