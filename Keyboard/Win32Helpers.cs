using System.Runtime.InteropServices;

namespace Keyboard;

internal static class Win32Helpers
{
    [DllImport("user32.dll", SetLastError = true)]
    internal static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr notificationFilter, DeviceNotification flags);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool UnregisterDeviceNotification(IntPtr handle);
}
