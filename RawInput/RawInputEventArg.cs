namespace RawInput;

public class RawInputEventArg : EventArgs
{
    public KeyPressEvent KeyPressEvent { get; private set; }

    public RawInputEventArg(KeyPressEvent arg)
    {
        KeyPressEvent = arg;
    }
}
