package com.leowaters.bluetooth;

import java.nio.ByteBuffer;

public interface BluetoothClientCallback
{
    void OnRecivedMessage(ByteBuffer array);
    void OnConnected();
    void OnDisconnect();
}
