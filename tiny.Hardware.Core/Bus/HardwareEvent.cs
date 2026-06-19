using System.Diagnostics;

namespace tiny.Hardware.Core.Bus
{
    [DebuggerStepThrough]
    public class HardwareEvent
    {
        public string ConfigKey { get; set; }
        public byte[] RawData { get; set; }
    }
}