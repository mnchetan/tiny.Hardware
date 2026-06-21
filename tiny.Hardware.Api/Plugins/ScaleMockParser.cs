using System.Diagnostics;
using System.Dynamic;
using System.Text;
using tiny.Hardware.Core.Extensions;
namespace tiny.Hardware.Api.Plugins
{
    /// <summary>
    /// The ScaleMockParser class is a mock implementation of the IProcessHardwareData interface, designed to simulate the parsing of raw hardware data from a serial weight scale. It processes byte arrays representing raw responses from an RS-232 scale and extracts meaningful information such as the stability of the weight reading and the extracted weight value. The parsed data is then returned in a structured format, including the device type, original scale output, extracted values, and a timestamp of when the processing occurred. This class serves as an example of how to implement custom data parsing logic for specific hardware types within the tiny.Hardware framework.
    /// </summary>
    // 3. SERIAL SCALES (Parses standard RS-232 stable weight strings)
    [DebuggerStepThrough]
    public class ScaleMockParser : IProcessHardwareData
    {
        /// <summary>
        /// PostProcessResponse takes a byte array representing the raw response from a serial weight scale and processes it to extract relevant information about the weight reading. It simulates parsing a string like "SCALE_STABLE_10KG" to determine the stability of the reading and the extracted weight value. If the raw response is not valid or does not contain enough data, it captures the original scale output for reference. The processed information is then returned as a JSON string, including the device type, extracted values, and a timestamp of when the processing occurred.
        /// </summary>
        /// <param name="rawResponse"></param>
        /// <returns></returns>
        public dynamic PostProcessResponse(byte[] rawResponse)
        {
            string rawText = Encoding.UTF8.GetString(rawResponse).Trim();
            dynamic result = new ExpandoObject();
            result.DeviceType = "Serial Weight Scale";
            result.ScaleOutput = rawText;
            // Simulate extracting just the numeric weight from a string like "SCALE_STABLE_10KG"
            result.IsStable = rawText.Contains("STABLE");
            result.ExtractedWeight = rawText.Replace("SCALE_STABLE_", "").Replace("KG", "");
            result.ProcessedAt = DateTime.UtcNow;
            return result.ToJSON();
        }
    }
}