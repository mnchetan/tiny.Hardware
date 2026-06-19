using System.Diagnostics;
using tiny.Hardware.Core.DataObjects;

namespace tiny.Hardware.Core.Configurations
{
    public interface ITinyHardwareConfigurations
    {
        [DebuggerHidden]
        Dictionary<string, HardwareSpecification> HardwareSpecifications { get; set; }
    }
}