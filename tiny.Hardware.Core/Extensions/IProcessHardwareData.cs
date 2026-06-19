namespace tiny.Hardware.Core.Extensions
{
    // The Interface vendors will implement
    public interface IProcessHardwareData
    {
        dynamic PostProcessResponse(byte[] rawResponse);
    }
}
