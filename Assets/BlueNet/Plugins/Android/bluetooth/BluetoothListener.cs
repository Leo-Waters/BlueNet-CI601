/*--------------------------------------------------------
 *  BlueNet A Bluetooth Multiplayers solution for unity.
 *  https://github.com/Leo-Waters/BlueNet-CI601
 * 
 *  Script: BlueToothListener
 *  Created by leo waters
 *  University Contact Email: l.waters3@uni.brighton.ac.uk
 *  Personal Contact Email li0nleo117@gmail.com
 * -------------------------------------------------------
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BlueNet.Android
{
    //bluetooth listener class for hosting a connection on android
    public class BluetoothListener : AndroidJavaProxy {
    
        const string JavaCallbackInterface = "com.leowaters.bluetooth.BluetoothListenerCallbacks";
		const string JavaClass =             "com.leowaters.bluetooth.BluetoothListener";
        AndroidJavaObject javaObject;
        public BluetoothListener() : base(JavaCallbackInterface)
        {
            javaObject = new AndroidJavaObject(JavaClass, this);
        }
        
        public event EventHandler<BluetoothClient> OnConnectedToClient;
        //on connected callback from java class to unity
        public void OnConnected(AndroidJavaObject client)
        {
            //create a bluetooth client object to represent the android java object and pass to c# event connected
            Debug.Log("Connection Successfull, Creating Client Object");
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