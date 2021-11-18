using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RawInput;

public sealed class RawKeyboard
{
    private readonly Dictionary<IntPtr, DeviceInformation> _deviceList = new();
    public int NumberOfKeyboards => _deviceList.Count;

    public delegate void DeviceEventHandler(object sender, RawInputEventArg e);
    public event DeviceEventHandler? KeyPressed;

    private InputData _rawBuffer;

    private readonly object _padLock = new();

    public RawKeyboard(IntPtr hwnd, bool captureOnlyInForeground)
    {
        var rid = new RawInputDevice[1];

        rid[0].UsagePage = HidUsagePage.GENERIC;
        rid[0].Usage = HidUsage.Keyboard;
        rid[0].Flags = (captureOnlyInForeground ? RawInputDeviceFlags.NONE : RawInputDeviceFlags.INPUTSINK) | RawInputDeviceFlags.DEVNOTIFY;
        rid[0].Target = hwnd;

        if (!Win32Helpers.RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(rid[0])))
        {
            throw new ApplicationException("Failed to register raw input device(s).");
        }
    }

    public void EnumerateDevices()
    {
        lock (_padLock)
        {
            _deviceList.Clear();

            foreach (var (deviceHandle, deviceInfo) in Win32.GetDevices())
            {
                _deviceList.Add(deviceHandle, deviceInfo);
            }

            Debug.WriteLine("EnumerateDevices() found {0} Keyboard(s)", NumberOfKeyboards);
        }
    }

    public void ProcessRawInput(IntPtr hdevice)
    {
        //Debug.WriteLine(_rawBuffer.data.keyboard.ToString());
        //Debug.WriteLine(_rawBuffer.data.hid.ToString());
        //Debug.WriteLine(_rawBuffer.header.ToString());

        if (_deviceList.Count == 0) 
            return;

        var dwSize = 0;
        var dwSizeResult = Win32Helpers.GetRawInputData(hdevice, DataCommand.RID_INPUT, IntPtr.Zero, ref dwSize, Marshal.SizeOf(typeof(Rawinputheader)));
        if (dwSizeResult != 0)
        {
            Debug.WriteLine("Error getting dw size");
            return;
        }

        if (dwSize != Win32Helpers.GetRawInputData(hdevice, DataCommand.RID_INPUT, out _rawBuffer, ref dwSize, Marshal.SizeOf(typeof(Rawinputheader))))
        {
            Debug.WriteLine("Error getting the rawinput buffer");
            return;
        }

        int virtualKey = _rawBuffer.data.keyboard.VKey;
        int makeCode = _rawBuffer.data.keyboard.Makecode;
        int flags = _rawBuffer.data.keyboard.Flags;

        if (virtualKey == Win32.KEYBOARD_OVERRUN_MAKE_CODE) 
            return;

        var isE0BitSet = (flags & Win32.RI_KEY_E0) != 0;

        DeviceInformation deviceInfo;

        if (_deviceList.ContainsKey(_rawBuffer.header.hDevice))
        {
            lock (_padLock)
            {
                deviceInfo = _deviceList[_rawBuffer.header.hDevice];
            }
        }
        else
        {
            Debug.WriteLine("Handle: {0} was not in the device list.", _rawBuffer.header.hDevice);
            return;
        }

        var isBreakBitSet = (flags & Win32.RI_KEY_BREAK) != 0;

        var keyPressEvent = new KeyPressEvent
        {
            DeviceHandle = _rawBuffer.header.hDevice,
            DeviceInformation = deviceInfo,
            KeyPressState = isBreakBitSet ? "BREAK" : "MAKE",
            Message = _rawBuffer.data.keyboard.Message,
            VKeyName = KeyMapper.GetKeyName(VirtualKeyCorrection(virtualKey, isE0BitSet, makeCode)).ToUpper(),
            VKey = virtualKey
        };

        KeyPressed?.Invoke(this, new RawInputEventArg(keyPressEvent));
    }

    private int VirtualKeyCorrection(int virtualKey, bool isE0BitSet, int makeCode)
    {
        var correctedVKey = virtualKey;

        if (_rawBuffer.header.hDevice == IntPtr.Zero)
        {
            // When hDevice is 0 and the vkey is VK_CONTROL indicates the ZOOM key
            if (_rawBuffer.data.keyboard.VKey == Win32.VK_CONTROL)
            {
                correctedVKey = Win32.VK_ZOOM;
            }
        }
        else
        {
            correctedVKey = virtualKey switch
            {
                // Right-hand CTRL and ALT have their e0 bit set 
                Win32.VK_CONTROL => isE0BitSet ? Win32.VK_RCONTROL : Win32.VK_LCONTROL,
                Win32.VK_MENU => isE0BitSet ? Win32.VK_RMENU : Win32.VK_LMENU,
                Win32.VK_SHIFT => makeCode == Win32.SC_SHIFT_R ? Win32.VK_RSHIFT : Win32.VK_LSHIFT,
                _ => virtualKey,
            };
        }

        return correctedVKey;
    }
}
