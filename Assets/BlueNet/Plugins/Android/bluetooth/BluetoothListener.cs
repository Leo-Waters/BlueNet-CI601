using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LeoWaters.android.bluetooth
{
    public class BluetoothListener : AndroidJavaProxy {
    
        const string JavaCallbackInterface = "com.leowaters.bluetooth.BluetoothListenerCallbacks";
		const string JavaClass =             "com.leowaters.bluetooth.BluetoothListener";
        AndroidJavaObject javaObject;
        public BluetoothListener() : base(JavaCallbackInterface)
        {
            javaObject = new AndroidJavaObject(JavaClass, this);
        }

        public event EventHandler<BluetoothClient> OnConnectedToClient;
        public void OnConnected(AndroidJavaObject client)
        {
            Debug.Log("Connection Successfull, Creatin Client Object");
            BluetoothClient bluetoothClient = new BluetoothClient(client);
            OnConnectedToClient?.Invoke(this, bluetoothClient);
        }

        public void StartListerning()
        {
            javaObject.Call("StartListerning");
        }

        public void StopListerning()
        {
            javaObject.Call("StopListerning");
        }
    }

    
}