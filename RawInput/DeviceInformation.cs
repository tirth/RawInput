namespace RawInput;

public record DeviceInformation
{
    public string Name { get; init; }        // i.e. \\?\HID#VID_045E&PID_00DD&MI_00#8&1eb402&0&0000#{884b96c3-56ef-11d1-bc8c-00a0c91405dd}
    
    public string Type { get; init; }        // KEYBOARD or HID

    public string Description { get; init; } // i.e. Microsoft USB Comfort Curve Keyboard 2000 (Mouse and Keyboard Center)
    
    public string Source { get; init; }
}


