package com.leowaters.bluetooth;

public interface BluetoothClientCallback
{
    void OnRecivedMessage(String message);
    void OnConnected();
    void OnDisconnect();
}
