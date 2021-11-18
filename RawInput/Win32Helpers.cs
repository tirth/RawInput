using System.Runtime.InteropServices;

namespace RawInput;

internal static class Win32Helpers
{
    [DllImport("user32.dll", SetLastError = true)]
    internal static extern int GetRawInputData(IntPtr hRawInput, DataCommand command, [Out] out InputData buffer, [In, Out] ref int size, int cbSizeHeader);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern int GetRawInputData(IntPtr hRawInput, DataCommand command, [Out] IntPtr pData, [In, Out] ref int size, int sizeHeader);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint GetRawInputDeviceInfo(IntPtr hDevice, RawInputDeviceInfo command, IntPtr pData, ref uint size);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint GetRawInputDeviceInfo(IntPtr hDevice, uint command, ref DeviceInfo data, ref uint dataSize);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint GetRawInputDeviceList(IntPtr pRawInputDeviceList, ref uint numberDevices, uint size);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool RegisterRawInputDevices(RawInputDevice[] pRawInputDevice, uint numberDevices, uint size);
}
