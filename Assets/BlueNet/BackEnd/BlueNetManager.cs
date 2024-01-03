using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using System;
namespace BlueNet
{
    //Provides Functionality for Creating and joining bluetooth Sessions
    public class BlueNetManager : MonoBehaviour
    {
        private void OnApplicationQuit()
        {
            if (IsConnected())
            {
                CloseClient();
            }
            if (IsHost())
            {
                CloseServer();
            }
        }

        static BlueNetManager Instance;

        public static bool IsConnected()
        {
            return BlueNetTransport.IsConnected();
        }

        public static bool IsHost()
        {
            return BlueNetTransport.IsHost();
        }

        //events-------------------------------------------------------------------------------------------------
        static IReadOnlyCollection<BluetoothDeviceInfo> DeviceInfos = null;
        public static event EventHandler<IReadOnlyCollection<BluetoothDeviceInfo>> RecivedNewDeviceList;

        public static event EventHandler OnDisconnected;

        public static event EventHandler OnConnected;

        

        //used to identify the application
        public int GUID = 1234;
        //scen you want to load on disconnect, -1 = none
        public int DisconectedSceneIndex = -1;

        //get a list of devices,if find new devices is true they will be returned via the RecivedDeviceList event to stop freezing main thread
        public static IEnumerable<BluetoothDeviceInfo> GetDevices(bool FindNewDevices)
        {

            if (FindNewDevices)
            {
                Thread DiscoverThread = new Thread(() =>
                {

                    List<BluetoothDeviceInfo> Devices = new List<BluetoothDeviceInfo>();
                    foreach (var item in BlueNetTransport.GetClient().DiscoverDevices())
                    {
                        if (!item.Authenticated)
                        {
                            Devices.Add(item);
                        }

                    }
                    DeviceInfos = Devices;
                });
                DiscoverThread.Start();
            }

            return BlueNetTransport.GetClient().PairedDevices;
        }

        //join a server
        public static void StartClient(BluetoothDeviceInfo device)
        {
            if (device.Authenticated)
            {
                BlueNetTransport.ConnectToDevice(device.DeviceAddress);
            }
            else
            {
                BluetoothSecurity.PairRequest(device.DeviceAddress);
            }
            
        }
        //close client
        public static void CloseClient()
        {
            BlueNetTransport.CloseConnection();
        }

        //start listerning for a joining client
        public static void StartServer()
        {
            if (BlueNetTransport.IsConnected() || BlueNetTransport.isListerningForClient())
            {
                Debug.LogWarning("Start Server Was Called but is connected or listing for a client");
            }
            BlueNetTransport.ListernForClient();
        }

        //
        public static void CloseServer()
        {
            if (IsConnected())
            {
                BlueNetTransport.CloseConnection();
            }
            else
            {
                BlueNetTransport.StopListerningForClient();
            }
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(Instance);

            BlueNetTransport.INIT(GUID);
            
        }

        private void Update()
        {
            //Send Device List event as couldnt be called on extra thread
            if (DeviceInfos!=null)
            {
                Debug.Log("New Device List Fetched!");
                RecivedNewDeviceList?.Invoke(Instance, DeviceInfos);
                DeviceInfos = null;
            }

            var messages = BlueNetTransport.GetMessages();
            foreach (string message in messages)
            {
                Debug.Log("Recived Message: "+message);
            }


            var events = BlueNetTransport.GetEvents(); //call events that are requested from other threads
            foreach(string Event in events)
            {
                if(Event== "connected")
                {
                    OnConnected?.Invoke(null, EventArgs.Empty);

                    if (IsHost())
                    {
                        BlueNetTransport.SendMessage("Message from Player 1");
                    }
                    else
                    {
                        BlueNetTransport.SendMessage("Message from Player 2");
                    }
                }
                else if(Event== "disconnected")
                {
                    if(DisconectedSceneIndex != -1)
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene(DisconectedSceneIndex);
                    }
                    
                    OnDisconnected?.Invoke(null, EventArgs.Empty);
                }
            }         

        }
    }
}
