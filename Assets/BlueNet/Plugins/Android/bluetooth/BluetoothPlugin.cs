using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

namespace BlueNet.Android
{
    //bluetooth plugin class for android, must be initalized before using clients of listner classes
    public class BluetoothPlugin
    {
        const string PluginName = "com.leowaters.bluetooth.BluetoothPlugin";

        static AndroidJavaClass java_bluetoothClass;
        static AndroidJavaObject java_bluetoothInstance;

        public static AndroidJavaClass GetBluetoothClass
        {
            get
            {
                if (java_bluetoothClass == null)
                {
                    java_bluetoothClass = new AndroidJavaClass(PluginName);
                }
                return java_bluetoothClass;
            }
        }

        public static AndroidJavaObject GetBluetoothObject
        {
            get
            {
                if (java_bluetoothInstance == null)
                {
                    java_bluetoothInstance = java_bluetoothClass.CallStatic<AndroidJavaObject>("GetInstance");
                }
                return java_bluetoothInstance;
            }
        }
        public static void Initalize(string appName, string uuid_Str)
        {
            var UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = UnityClass.GetStatic<AndroidJavaObject>("currentActivity");

            GetBluetoothClass.CallStatic("Init", activity, appName,uuid_Str);
        }

        //dispose of resources 
        public static void Dispose()
        {
            GetBluetoothClass.CallStatic("Dispose");
        }

        //performs a native android toast message
        public static void Toast(string message)
        {
            GetBluetoothClass.CallStatic("PerformToast", message);
        }

        //key is device name, value is address
        public static Dictionary<string,string> GetDevices()
        {
            Dictionary<string, string> devices = new Dictionary<string, string>();
            string[] deviceInfo=GetBluetoothClass.CallStatic<string[]>("GetPairedDevices");
            for (int i = 0; i < deviceInfo.Length; i+=2)
            {
                if (devices.ContainsKey(deviceInfo[i]) == false)
                {
                    devices.Add(deviceInfo[i], deviceInfo[i + 1]);
                }
            }
            return devices;
        }

    }
}
