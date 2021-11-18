using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace RawInput;

static public class Win32
{
    public static int LoWord(int dwValue) 
        => dwValue & 0xFFFF;

    public static int HiWord(long dwValue) 
        => (int)(dwValue >> 16) & ~FAPPCOMMANDMASK;

    public static ushort LowWord(uint val) 
        => (ushort)val;

    public static ushort HighWord(uint val) 
        => (ushort)(val >> 16);

    public static uint BuildWParam(ushort low, ushort high) 
        => (uint)high << 16 | low;

    // ReSharper disable InconsistentNaming
    public const int KEYBOARD_OVERRUN_MAKE_CODE = 0xFF;
    public const int WM_APPCOMMAND = 0x0319;
    private const int FAPPCOMMANDMASK = 0xF000;
    internal const int FAPPCOMMANDMOUSE = 0x8000;
    internal const int FAPPCOMMANDOEM = 0x1000;

    public const int WM_KEYDOWN = 0x0100;
    public const int WM_KEYUP = 0x0101;
    public const int WM_SYSKEYDOWN = 0x0104;
    public const int WM_INPUT = 0x00FF;
    public const int WM_USB_DEVICECHANGE = 0x0219;

    internal const int VK_SHIFT = 0x10;

    internal const int RI_KEY_MAKE = 0x00;      // Key Down
    internal const int RI_KEY_BREAK = 0x01;     // Key Up
    internal const int RI_KEY_E0 = 0x02;        // Left version of the key
    internal const int RI_KEY_E1 = 0x04;        // Right version of the key. Only seems to be set for the Pause/Break key.

    internal const int VK_CONTROL = 0x11;
    internal const int VK_MENU = 0x12;
    internal const int VK_ZOOM = 0xFB;
    internal const int VK_LSHIFT = 0xA0;
    internal const int VK_RSHIFT = 0xA1;
    internal const int VK_LCONTROL = 0xA2;
    internal const int VK_RCONTROL = 0xA3;
    internal const int VK_LMENU = 0xA4;
    internal const int VK_RMENU = 0xA5;

    internal const int SC_SHIFT_R = 0x36;
    internal const int SC_SHIFT_L = 0x2a;
    internal const int RIM_INPUT = 0x00;

    public static void DeviceAudit(string outputPath = "device_info.json")
    {
        File.WriteAllText(outputPath, GetDevices().Values.ToList().JsonStr());
    }

    public static Dictionary<IntPtr, DeviceInformation> GetDevices()
    {
        var devices = new Dictionary<IntPtr, DeviceInformation>();

        var dwSize = Marshal.SizeOf(typeof(Rawinputdevicelist));

        // get device count
        uint deviceCount = 0;
        if (Win32Helpers.GetRawInputDeviceList(IntPtr.Zero, ref deviceCount, (uint)dwSize) != 0)
            throw new Win32Exception(Marshal.GetLastWin32Error());

        Trace.WriteLine($"device count: {deviceCount}");

        // read list
        var pRawInputDeviceList = Marshal.AllocHGlobal((int)(dwSize * deviceCount));
        var readIntoList = Win32Helpers.GetRawInputDeviceList(pRawInputDeviceList, ref deviceCount, (uint)dwSize);

        Trace.WriteLine($"read into list: {readIntoList}");

        var keyboardNumber = 0;

        var globalDevice = new DeviceInformation
        {
            Name = "Global Keyboard",
            Type = Win32.GetDeviceType(DeviceType.RimTypekeyboard),
            Description = "Fake Keyboard. Some keys (ZOOM, MUTE, VOLUMEUP, VOLUMEDOWN) are sent to rawinput with a handle of zero.",
            Source = $"Keyboard_{keyboardNumber++:D2}"
        };

        devices.Add(IntPtr.Zero, globalDevice);

        for (var i = 0; i < deviceCount; i++)
        {
            // On Window 8 64bit when compiling against .Net > 3.5 using .ToInt32 you will generate an arithmetic overflow. Leave as it is for 32bit/64bit applications
            if (Marshal.PtrToStructure(new IntPtr(pRawInputDeviceList.ToInt64() + dwSize * i), typeof(Rawinputdevicelist)) is not Rawinputdevicelist rawInputDevice)
            {
                Trace.WriteLine($"ERROR marshalling device {i} to structure");
                continue;
            }

            uint pcbSize = 0;

            var pcbSizeResult = Win32Helpers.GetRawInputDeviceInfo(rawInputDevice.hDevice, RawInputDeviceInfo.RIDI_DEVICENAME, IntPtr.Zero, ref pcbSize);

            if (pcbSizeResult != 0)
            {
                Trace.WriteLine($"ERROR getting pcb size for device {i}, got {pcbSizeResult}, {Marshal.GetLastWin32Error()}");
                continue;
            }

            if (pcbSize <= 0)
            {
                Trace.WriteLine($"ERROR getting pcb size for device {i}, got {pcbSize}, {Marshal.GetLastWin32Error()}");
                continue;
            }

            var deviceInfoSize = (uint)Marshal.SizeOf(typeof(DeviceInfo));
            var deviceInfo = new DeviceInfo 
            { 
                Size = (int)deviceInfoSize 
            };

            var deviceInfoResult = Win32Helpers.GetRawInputDeviceInfo(rawInputDevice.hDevice, (uint)RawInputDeviceInfo.RIDI_DEVICEINFO, ref deviceInfo, ref deviceInfoSize);
            if (deviceInfoResult <= 0)
            {
                Trace.WriteLine($"ERROR getting device info for device {i}, got {deviceInfoResult}, {Marshal.GetLastWin32Error()}");
                continue;
            }

            Trace.WriteLine($"{deviceInfo}");
            Trace.WriteLine($"{deviceInfo.HIDInfo}");
            Trace.WriteLine($"{deviceInfo.KeyboardInfo}");

            var deviceNamePtr = Marshal.AllocHGlobal((int)pcbSize);

            var deviceNameResult = Win32Helpers.GetRawInputDeviceInfo(rawInputDevice.hDevice, RawInputDeviceInfo.RIDI_DEVICENAME, deviceNamePtr, ref pcbSize);
            if (deviceNameResult <= 0)
            {
                Trace.WriteLine($"ERROR getting device name for device {i}, got {deviceNameResult}, {Marshal.GetLastWin32Error()}");
                continue;
            }

            var deviceName = Marshal.PtrToStringAnsi(deviceNamePtr);
            if (string.IsNullOrEmpty(deviceName))
            {
                Trace.WriteLine($"ERROR getting device name for device {i}, {Marshal.GetLastWin32Error()}");
                continue;
            }

            Marshal.FreeHGlobal(deviceNamePtr);

            if (rawInputDevice.dwType == DeviceType.RimTypemouse)
            {
                Trace.WriteLine($"skipping mouse");
                continue;
            }

            var deviceInformation = new DeviceInformation
            {
                Name = deviceName,
                Type = GetDeviceType(rawInputDevice.dwType),
                Description = GetDeviceDescription(deviceName),
                Source = $"Keyboard_{keyboardNumber++:D2}"
            };

            Trace.WriteLine($"device: {deviceInformation}");

            // TODO: overwrite entry instead?
            if (!devices.ContainsKey(rawInputDevice.hDevice))
                devices.Add(rawInputDevice.hDevice, deviceInformation);

            Trace.WriteLine("========================");
        }

        Marshal.FreeHGlobal(pRawInputDeviceList);

        return devices;
    }

    public static string GetDeviceType(uint device) => device switch
    {
        DeviceType.RimTypemouse => "MOUSE",
        DeviceType.RimTypekeyboard => "KEYBOARD",
        DeviceType.RimTypeHid => "HID",
        _ => "UNKNOWN",
    };

    public static string GetDeviceDescription(string device)
    {
        try
        {
            var desc = RegistryAccess.GetDeviceKey(device)?.GetValue("DeviceDesc")?.ToString();
            if (string.IsNullOrEmpty(desc))
                return "Unknown";

            return desc[(desc.IndexOf(';') + 1)..];
        }
        catch (Exception e)
        {
            return $"Device is malformed unable to look up in the registry: {e.Message}";
        }

        //var deviceClass = RegistryAccess.GetClassType(deviceKey.GetValue("ClassGUID").ToString());
        //isKeyboard = deviceClass.ToUpper().Equals( "KEYBOARD" );
    }

    //public static bool InputInForeground(IntPtr wparam)
    //{
    //    return wparam.ToInt32() == RIM_INPUT;
    //}
}
