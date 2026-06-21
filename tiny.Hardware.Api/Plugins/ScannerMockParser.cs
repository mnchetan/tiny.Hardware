using System.Diagnostics;
using System.Dynamic;
using System.Text;
using tiny.Hardware.Core.Extensions;
namespace tiny.Hardware.Api.Plugins
{
    /// <summary>
    /// The ScannerMockParser class is a mock implementation of the IProcessHardwareData interface, designed to simulate the parsing of raw hardware data from an overhead scanner. It processes byte arrays representing raw responses from a scanner device and extracts meaningful information such as the box ID and weight reading. The parsed data is then returned in a structured format, including the device type, original raw payload, extracted values, and a timestamp of when the processing occurred. This class serves as an example of how to implement custom data parsing logic for specific hardware types within the tiny.Hardware framework.
    /// </summary>
    // 1. SCANNERS (Parses the string "BOX_ID_0001|WEIGHT_20KG" from your Simulator)
    [DebuggerStepThrough]
    public class ScannerMockParser : IProcessHardwareData
    {
        /// <summary>
        /// PostProcessResponse takes a byte array representing the raw response from an overhead scanner and processes it to extract relevant information about the scanned item. It simulates parsing a string like "BOX_ID_0001|WEIGHT_20KG" to determine the box ID and weight reading. If the raw response is not valid or does not contain enough data, it captures the original raw payload for reference. The processed information is then returned as a JSON string, including the device type, extracted values, and a timestamp of when the processing occurred.
        /// </summary>
        /// <param name="rawResponse"></param>
        /// <returns></returns>
        public dynamic PostProcessResponse(byte[] rawResponse)
        {
            string rawText = Encoding.ASCII.GetString(rawResponse).Trim();
            dynamic result = new ExpandoObject();
            result.DeviceType = "Overhead Scanner";
            result.RawPayload = rawText;
            // Simulate business logic parsing the pipe-delimited simulator data
            string[] parts = rawText.Split('|');
            if (parts.Length == 2)
            {
                result.BoxId = parts[0];
                result.WeightRead = parts[1];
            }
            result.ProcessedAt = DateTime.UtcNow;
            return result.ToJSON();
        }
    }
}