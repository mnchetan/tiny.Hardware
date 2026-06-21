namespace tiny.Hardware.Api.DataObjects
{
    /// <summary>
    /// The HardwareWritePayload class represents the data structure for writing commands to a hardware device. It contains a Data property, which holds the command or information to be sent to the device, and an EncodingFormat property, which specifies the format in which the data should be encoded (e.g., "ASCII", "HEX", "UTF8"). This class is used as the payload for API requests that involve sending commands to hardware devices, allowing clients to specify both the content and the encoding format of the data being transmitted.
    /// </summary>
    // Payload Model
    public class HardwareWritePayload
    {
        /// <summary>
        /// The Data property contains the command or information to be sent to the hardware device. It is a string that represents the content of the command, which will be encoded according to the specified EncodingFormat before being transmitted to the device. This property is essential for defining what action or instruction is intended for the hardware interaction.
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// The EncodingFormat property specifies the format in which the data should be encoded before being sent to the hardware device. It supports values like "ASCII", "HEX", and "UTF8". This property is used to determine how the Data property should be converted into a byte array for transmission.
        /// </summary>
        // "ASCII", "HEX", "UTF8"
        public string EncodingFormat { get; set; } = "ASCII";
    }
}
