﻿namespace Keyboard;

internal struct BroadcastDeviceInterface
{
    public int DbccSize;
    public BroadcastDeviceType BroadcastDeviceType;
    public int DbccReserved;
    public Guid DbccClassguid;
    public char DbccName;
}