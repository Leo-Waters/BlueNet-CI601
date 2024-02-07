package com.leowaters.bluetooth;

public interface BluetoothClientCallback
{
    void OnRecivedMessage(byte[] message);
    void OnConnected();
    void OnDisconnect();
}
