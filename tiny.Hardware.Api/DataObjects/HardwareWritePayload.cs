namespace tiny.Hardware.Api.DataObjects
{
    // Payload Model
    public class HardwareWritePayload
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public string Data { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        // "ASCII", "HEX", "UTF8"
        public string EncodingFormat { get; set; } = "ASCII";
    }
}
