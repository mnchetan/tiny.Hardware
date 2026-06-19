using System.Diagnostics;
using System.Dynamic;
using System.Text;
using tiny.Hardware.Core.Extensions;

namespace tiny.Hardware.Api.Plugins
{
    // 1. SCANNERS (Parses the string "BOX_ID_0001|WEIGHT_20KG" from your Simulator)
    [DebuggerStepThrough]
    public class ScannerMockParser : IProcessHardwareData
    {
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