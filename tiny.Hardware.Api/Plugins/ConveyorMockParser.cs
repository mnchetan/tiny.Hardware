using System.Diagnostics;
using System.Dynamic;
using tiny.Hardware.Core.Extensions;

namespace tiny.Hardware.Api.Plugins
{
    // 2. MODBUS PLCs (Parses raw Hexadecimal Register arrays)
    [DebuggerStepThrough]
    public class ConveyorMockParser : IProcessHardwareData
    {
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
                result.RawHex = BitConverter.ToString(rawResponse).Replace("-", "");
                result.Status = "Idle";
            }

            result.ProcessedAt = DateTime.UtcNow;
            return result.ToJSON();
        }
    }
}