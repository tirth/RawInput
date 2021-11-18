using System.Diagnostics;
using System.Globalization;
using RawInput;

namespace Keyboard;

public partial class Keyboard : Form
{
    private readonly RawInput _rawInput;

    const bool CaptureOnlyInForeground = true;
    // Todo: add checkbox to form when checked/uncheck create method to call that does the same as Keyboard ctor 

    public Keyboard()
    {
        InitializeComponent();
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        _rawInput = new RawInput(Handle, CaptureOnlyInForeground);

        _rawInput.AddMessageFilter();   // Adding a message filter will cause keypresses to be handled
        Win32.DeviceAudit();            // Writes a file device_info.json to the current directory

        _rawInput.KeyPressed += OnKeyPressed;
    }

    private void OnKeyPressed(object sender, RawInputEventArg e)
    {
        lbHandle.Text = e.KeyPressEvent.DeviceHandle.ToString();
        lbType.Text = e.KeyPressEvent.DeviceInformation.Type;
        lbName.Text = e.KeyPressEvent.DeviceInformation.Name;
        lbDescription.Text = e.KeyPressEvent.DeviceInformation.Description;
        lbKey.Text = e.KeyPressEvent.VKey.ToString(CultureInfo.InvariantCulture);
        lbNumKeyboards.Text = _rawInput.NumberOfKeyboards.ToString(CultureInfo.InvariantCulture);
        lbVKey.Text = e.KeyPressEvent.VKeyName;
        lbSource.Text = e.KeyPressEvent.DeviceInformation.Source;
        lbKeyPressState.Text = e.KeyPressEvent.KeyPressState;
        lbMessage.Text = string.Format("0x{0:X4} ({0})", e.KeyPressEvent.Message);

        //switch (e.KeyPressEvent.Message)
        //{
        //    case Win32.WM_KEYDOWN:
        //        Debug.WriteLine(e.KeyPressEvent.KeyPressState);
        //        break;
        //     case Win32.WM_KEYUP:
        //        Debug.WriteLine(e.KeyPressEvent.KeyPressState);
        //        break;
        //}
    }

    private void Keyboard_FormClosing(object sender, FormClosingEventArgs e)
    {
        _rawInput.KeyPressed -= OnKeyPressed;
    }

    private static void CurrentDomain_UnhandledException(Object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is not Exception ex) 
            return;

        // Log this error. Logging the exception doesn't correct the problem but at least now
        // you may have more insight as to why the exception is being thrown.
        Debug.WriteLine("Unhandled Exception: " + ex.Message);
        Debug.WriteLine("Unhandled Exception: " + ex);
        MessageBox.Show(ex.Message);
    }
}
