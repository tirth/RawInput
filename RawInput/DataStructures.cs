﻿using System.Runtime.InteropServices;

namespace RawInput;

[StructLayout(LayoutKind.Explicit)]
public struct DeviceInfo
{
    [FieldOffset(0)]
    public int Size;
    [FieldOffset(4)]
    public int Type;
    [FieldOffset(8)]
    public DeviceInfoMouse MouseInfo;
    [FieldOffset(8)]
    public DeviceInfoKeyboard KeyboardInfo;
    [FieldOffset(8)]
    public DeviceInfoHid HIDInfo;

    public override string ToString() 
        => $"DeviceInfo\n Size: {Size}\n Type: {Type}\n";
}

public struct DeviceInfoMouse
{
    public uint Id;                         // Identifier of the mouse device
    public uint NumberOfButtons;            // Number of buttons for the mouse
    public uint SampleRate;                 // Number of data points per second.
    public bool HasHorizontalWheel;         // True is mouse has wheel for horizontal scrolling else false.
                                            // ReSharper restore MemberCanBePrivate.Global
    public override string ToString() 
        => $"MouseInfo\n Id: {Id}\n NumberOfButtons: {NumberOfButtons}\n SampleRate: {SampleRate}\n HorizontalWheel: {HasHorizontalWheel}\n";
}

public struct DeviceInfoKeyboard
{
    public uint Type;                       // Type of the keyboard
    public uint SubType;                    // Subtype of the keyboard
    public uint KeyboardMode;               // The scan code mode
    public uint NumberOfFunctionKeys;       // Number of function keys on the keyboard
    public uint NumberOfIndicators;         // Number of LED indicators on the keyboard
    public uint NumberOfKeysTotal;          // Total number of keys on the keyboard

    public override string ToString() 
        => $"DeviceInfoKeyboard\n Type: {Type}\n SubType: {SubType}\n KeyboardMode: {KeyboardMode}\n NumberOfFunctionKeys: {NumberOfFunctionKeys}\n NumberOfIndicators {NumberOfIndicators}\n NumberOfKeysTotal: {NumberOfKeysTotal}\n";
}

public struct DeviceInfoHid
{
    public uint VendorID;       // Vendor identifier for the HID
    public uint ProductID;      // Product identifier for the HID
    public uint VersionNumber;  // Version number for the device
    public ushort UsagePage;    // Top-level collection Usage page for the device
    public ushort Usage;        // Top-level collection Usage for the device

    public override string ToString() 
        => $"HidInfo\n VendorID: {VendorID}\n ProductID: {ProductID}\n VersionNumber: {VersionNumber}\n UsagePage: {UsagePage}\n Usage: {Usage}\n";
}

public struct BroadcastDeviceInterface
{
    public int DbccSize;
    public BroadcastDeviceType BroadcastDeviceType;
    public int DbccReserved;
    public Guid DbccClassguid;
    public char DbccName;
}

[StructLayout(LayoutKind.Sequential)]
internal struct Rawinputdevicelist
{
    public IntPtr hDevice;
    public uint dwType;
}

[StructLayout(LayoutKind.Explicit)]
public struct RawData
{
    [FieldOffset(0)]
    internal Rawmouse mouse;
    [FieldOffset(0)]
    internal Rawkeyboard keyboard;
    [FieldOffset(0)]
    internal Rawhid hid;
}

[StructLayout(LayoutKind.Sequential)]
public struct InputData
{
    public Rawinputheader header;           // 64 bit header size: 24  32 bit the header size: 16
    public RawData data;                    // Creating the rest in a struct allows the header size to align correctly for 32/64 bit
}

[StructLayout(LayoutKind.Sequential)]
public struct Rawinputheader
{
    public uint dwType;                     // Type of raw input (RIM_TYPEHID 2, RIM_TYPEKEYBOARD 1, RIM_TYPEMOUSE 0)
    public uint dwSize;                     // Size in bytes of the entire input packet of data. This includes RAWINPUT plus possible extra input reports in the RAWHID variable length array. 
    public IntPtr hDevice;                  // A handle to the device generating the raw input data. 
    public IntPtr wParam;                   // RIM_INPUT 0 if input occurred while application was in the foreground else RIM_INPUTSINK 1 if it was not.

    public override string ToString() 
        => $"RawInputHeader\n dwType : {dwType}\n dwSize : {dwSize}\n hDevice : {hDevice}\n wParam : {wParam}";
}

[StructLayout(LayoutKind.Sequential)]
internal struct Rawhid
{
    public uint dwSizHid;
    public uint dwCount;
    public byte bRawData;

    public override string ToString() 
        => $"Rawhid\n dwSizeHid : {dwSizHid}\n dwCount : {dwCount}\n bRawData : {bRawData}\n";
}

[StructLayout(LayoutKind.Explicit)]
internal struct Rawmouse
{
    [FieldOffset(0)]
    public ushort usFlags;
    [FieldOffset(4)]
    public uint ulButtons;
    [FieldOffset(4)]
    public ushort usButtonFlags;
    [FieldOffset(6)]
    public ushort usButtonData;
    [FieldOffset(8)]
    public uint ulRawButtons;
    [FieldOffset(12)]
    public int lLastX;
    [FieldOffset(16)]
    public int lLastY;
    [FieldOffset(20)]
    public uint ulExtraInformation;
}

[StructLayout(LayoutKind.Sequential)]
internal struct Rawkeyboard
{
    public ushort Makecode;                 // Scan code from the key depression
    public ushort Flags;                    // One or more of RI_KEY_MAKE, RI_KEY_BREAK, RI_KEY_E0, RI_KEY_E1
    private readonly ushort Reserved;       // Always 0    
    public ushort VKey;                     // Virtual Key Code
    public uint Message;                    // Corresponding Windows message for exmaple (WM_KEYDOWN, WM_SYASKEYDOWN etc)
    public uint ExtraInformation;           // The device-specific addition information for the event (seems to always be zero for keyboards)

    public override string ToString() 
        => $"Rawkeyboard\n Makecode: {Makecode}\n Makecode(hex) : {Makecode:X}\n Flags: {Flags}\n Reserved: {Reserved}\n VKeyName: {VKey}\n Message: {Message}\n ExtraInformation {ExtraInformation}\n";
}

[StructLayout(LayoutKind.Sequential)]
internal struct RawInputDevice
{
    internal HidUsagePage UsagePage;
    internal HidUsage Usage;
    internal RawInputDeviceFlags Flags;
    internal IntPtr Target;

    public override string ToString() 
        => $"{UsagePage}/{Usage}, flags: {Flags}, target: {Target}";
}
