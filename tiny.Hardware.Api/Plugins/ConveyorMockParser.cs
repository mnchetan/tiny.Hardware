using System.Diagnostics;
using System.Dynamic;
using tiny.Hardware.Core.Extensions;
namespace tiny.Hardware.Api.Plugins
{
    /// <summary>
    /// The ConveyorMockParser class is a mock implementation of the IProcessHardwareData interface, designed to simulate the parsing of raw hardware data from a conveyor system. It processes byte arrays representing raw responses from a Modbus PLC (Programmable Logic Controller) and extracts meaningful information such as conveyor speed and status. The parsed data is then returned in a structured format, including the device type, processed values, and a timestamp of when the processing occurred. This class serves as an example of how to implement custom data parsing logic for specific hardware types within the tiny.Hardware framework.
    /// </summary>
    // 2. MODBUS PLCs (Parses raw Hexadecimal Register arrays)
    [DebuggerStepThrough]
    public class ConveyorMockParser : IProcessHardwareData
    {
        /// <summary>
        /// PostProcessResponse takes a byte array representing the raw response from a Modbus PLC and processes it to extract relevant information about the conveyor system. It simulates reading a 16-bit register to determine the conveyor speed in RPM and sets the status of the conveyor (e.g., "Running" or "Idle"). If the raw response is not valid or does not contain enough data, it captures the raw hexadecimal string for reference. The processed information is then returned as a JSON string, including the device type, extracted values, and a timestamp of when the processing occurred.
        /// </summary>
        /// <param name="rawResponse"></param>
        /// <returns></returns>
        public dynamic PostProcessResponse(byte[] rawResponse)
        {
            dynamic result = new ExpandoObject();
            result.DeviceType = "Modbus PLC";
            // Simulating reading a 16-bit register
            if (rawResponse != null && rawResponse.Length >= 2)
            {
                // Convert bytes to a readable integer (e.g., Conveyor Speed)
                if (BitConverter.IsLittleEndian) Array.Reverse(rawResponse);
                result.ConveyorSpeedRpm = BitConverter.ToInt16(rawResponse, 0);
                result.Status = "Running";
            }
            else
            {
                result.RawHex = Convert.ToHexString(rawResponse);
                result.Status = "Idle";
            }
            result.ProcessedAt = DateTime.UtcNow;
            return result.ToJSON();
        }
    }
}