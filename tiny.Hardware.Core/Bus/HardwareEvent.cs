using System.Diagnostics;
namespace tiny.Hardware.Core.Bus
{
    /// <summary>
    /// The HardwareEvent class represents an event that is published on the InternalHardwareBus when hardware data is received. It contains a configuration key that identifies the type of hardware event and a byte array of raw data that represents the information received from the hardware device. This class serves as the basic data structure for events that are processed by the HardwareProcessorService, allowing for dynamic handling of different hardware types based on their configuration keys and associated raw data.
    /// </summary>
    [DebuggerStepThrough]
    public class HardwareEvent
    {
        /// <summary>
        /// ConfigKey is a string that identifies the specific hardware configuration associated with this event. It is used to look up the corresponding HardwareSpecification from ITinyHardwareConfigurations, which contains the details about how to process the raw data for this particular hardware type. The ConfigKey allows the HardwareProcessorService to dynamically determine how to handle the incoming hardware data based on the defined specifications and any associated post-processing plugins.
        /// </summary>
        public string ConfigKey { get; set; }
        /// <summary>
        /// RawData is a byte array that contains the raw information received from the hardware device. This data is typically in a format that requires decoding or processing to extract meaningful information. The HardwareProcessorService uses the ConfigKey to determine how to interpret this raw data, either by applying a custom post-processing plugin or by using a default decoding mechanism based on the specified encoding type in the hardware configuration. The RawData property serves as the primary payload of the hardware event, containing the unprocessed information that needs to be handled according to the defined specifications.
        /// </summary>
        public byte[] RawData { get; set; }
    }
}