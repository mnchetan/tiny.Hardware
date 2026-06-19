using System.Diagnostics;
using System.Dynamic;
using System.Text;
using tiny.Hardware.Core.Extensions;

namespace tiny.Hardware.Api.Plugins
{
    // 3. SERIAL SCALES (Parses standard RS-232 stable weight strings)
    [DebuggerStepThrough]
    public class ScaleMockParser : IProcessHardwareData
    {
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