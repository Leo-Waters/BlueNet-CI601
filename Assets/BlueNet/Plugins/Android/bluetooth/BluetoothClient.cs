using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlueNet.Android
{
	//bluetooth client class for android
	public class BluetoothClient : AndroidJavaProxy
    {
		const string JavaCallbackInterface = "com.leowaters.bluetooth.BluetoothClientCallback";
		const string JavaClass = "com.leowaters.bluetooth.BluetoothClient";


		private bool p_isConnected;
		public bool isConnected
		{
			get
			{
				return p_isConnected;
			}
		}

		AndroidJavaObject javaObject;
		public BluetoothClient() : base(JavaCallbackInterface)
		{
			javaObject = new AndroidJavaObject(JavaClass, this);
		}
		//create c# proxy client object from returned java object
		public BluetoothClient(AndroidJavaObject client) : base(JavaCallbackInterface)
		{
			javaObject = client;
			client.Call("SetCallback", this);
		}

		//connect to a device by address
		public void StartClient(string DeviceAddress)
		{
			javaObject.Call("StartClient", DeviceAddress);
		}

		//close connection to client
		public void Disconnect()
		{
			javaObject.Call("Disconnect");
		}

		//Send bytes to client
		public void Write(byte[] data)
		{
			javaObject.Call("Write", data);
		}


		//callbacks-------------------------------------------

		//recived a message from client
		public event EventHandler<string> onRecivedMessage_Evt;
		//connected to client
		public event EventHandler onConnected_Evt;
		//connection was closed
		public event EventHandler onDisconnect_Evt;

		//onRecivedJavaCallback
		public void OnRecivedMessage(string message)
		{
			onRecivedMessage_Evt?.Invoke(this, message);
		}
		//OnConnectedJavaCallback
		public void OnConnected()
		{
			p_isConnected = true;
			onConnected_Evt?.Invoke(this,EventArgs.Empty);

		}
		//OnDisconnectJavaCallback
		public void OnDisconnect()
		{
			p_isConnected = false;
			onDisconnect_Evt?.Invoke(this, EventArgs.Empty);
		}


	}
}