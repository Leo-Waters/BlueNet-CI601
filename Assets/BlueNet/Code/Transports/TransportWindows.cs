#if UNITY_EDITOR||UNITY_STANDALONE_WIN
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Android;
using BlueNet.DataTypes;
namespace BlueNet.Transports
{
    //transport for windows, using 32feet.net
    public class BlueNetTransportWindows : BlueNetTransportBase
    {
        public BlueNetTransportWindows(int _GUID) : base(_GUID)
        {
            client = new BluetoothClient();
        }
         BluetoothClient client;
         BluetoothListener bluetoothListener;

        Thread AcceptClientThread, NetworkReadThread;

        //return paired device list
        public override Dictionary<string,string> GetDevices()
        {
            var devices= new Dictionary<string, string>();
            foreach (var device in client.PairedDevices)
            {
                devices.Add(device.DeviceName, device.DeviceAddress.ToString());
            }
            return devices;
        }
        //connect to device
        public override void ConnectToDevice(string address)
        {
            client = new BluetoothClient();
            client.Connect(BluetoothAddress.Parse(address), GetGuid());

            if (client.Connected)
            {
                Connected = true;
                StartReadThread();
                
            }
            else
            {
                Debug.Log("Failed To Connect");
                client.Close();
            }


        }

        //start thread waiting for client
        public override void ListernForClient()
        {
            isHost =IsListerning = true;
            Debug.Log("Waiting For BlueToothConnection");

            if(bluetoothListener!=null&& bluetoothListener.Active)//clean up un disposed listerner;
            {
                Debug.LogWarning("Listener wasnt closed");
                StopListerningForClient();
            }

            bluetoothListener = new BluetoothListener(GetGuid());

            bluetoothListener.Start();

            AcceptClientThread = new Thread(AcceptClient);
            AcceptClientThread.Start();
        }

        //cancel thread waiting for client
        public override void StopListerningForClient()
        {
            if (AcceptClientThread != null)
            {
                Debug.Log("No Longer listerning for client");
                AcceptClientThread.Abort();
                bluetoothListener.Stop();
                AcceptClientThread = null;
            }
            IsListerning = false;

            if (IsConnected() == false)
            {
                isHost = false;
            }
        }

        //Accept client thread, waits for client to join
        void AcceptClient()
        {
            client = bluetoothListener.AcceptBluetoothClient();
            Connected = true;
            bluetoothListener.Stop();
            StartReadThread();
        }
        //closes connection to client
        public override void CloseConnection()  { NetworkReadThread.Abort();  client.Close();  ThreadSafeEvents.Add("disconnected"); base.CloseConnection();  }
        
        //start read and write background threads for data transmision
        void StartReadThread()
        {
            //clean up old data
            Send_Messages.Clear();
            NetworkCommands.Clear();
            ThreadSafeEvents.Clear();

            ThreadSafeEvents.Add("connected");
            NetworkReadThread = new Thread(ReadThread);

            NetworkReadThread.Start();
        }

        //reads incoming data from client
        void ReadThread()
        {
            var stream = client.GetStream();

            byte[] ReadBuffer = new byte[1024];
            while (client.Connected&&Connected && stream != null)
            {
                if (stream.CanRead)
                {
                    int bytesRead = stream.Read(ReadBuffer, 0, ReadBuffer.Length);
                    if (bytesRead <= 0)
                    {
                        Debug.Log("Client Disconect detected");
                        client.Close();
                        Connected = false;
                        ThreadSafeEvents.Add("disconnected");
                        return;
                    }

                    byte[] message = new byte[bytesRead];
                    Array.Copy(ReadBuffer,message,bytesRead);

                    ProccessMessage(message);

                }
            }
            if (stream != null)//client might have been disposed, clearing the stream already
            {
                stream.Close();
            }
        }

        //send Messages to client from Message buffer
        public override void Send()
        {
            if (Connected)
            {
                var stream = client.GetStream();
                if (stream.CanWrite)
                {
                    byte[] message = GetMessage();
                    if (message != null)
                    {
                        stream.Write(message, 0, message.Length);
                        TotalBytesSent += message.Length;
                    }

                }
            }

        }
    }

}
#endif