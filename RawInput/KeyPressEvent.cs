namespace RawInput;

public record KeyPressEvent
{
    public DeviceInformation DeviceInformation { get; init; }
    public IntPtr DeviceHandle;     // Handle to the device that send the input
    public int VKey;                // Virtual Key. Corrected for L/R keys(i.e. LSHIFT/RSHIFT) and Zoom
    public string VKeyName;         // Virtual Key Name. Corrected for L/R keys(i.e. LSHIFT/RSHIFT) and Zoom
    public uint Message;            // WM_KEYDOWN or WM_KEYUP        
    public string KeyPressState;    // MAKE or BREAK
}
