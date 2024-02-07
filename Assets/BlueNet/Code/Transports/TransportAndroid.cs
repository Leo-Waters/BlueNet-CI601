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
using BlueNet.Android;
namespace BlueNet.Transports
{
    //bluenet transport for android
    public class BlueNetTransportAndroid:BlueNetTransportBase
    {
        //init transport with a GUID to differentiate the application when joining
        BluetoothClient client;
        BluetoothListener listener;
        public BlueNetTransportAndroid(int _GUID) : base(_GUID)
        {
            BluetoothPlugin.Initalize("BlueNet", GetGuid().ToString());
            listener = new BluetoothListener();
            listener.OnConnectedToClient += Listener_OnConnectedToClient;

            //prevent android device from sleeping, causes disconnects, desync and other odd network behaviour
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        //send all messages
        public override void Send()
        {
            if (Connected)
            {
                byte[] message = GetMessage();
                if (message!=null)
                {
                    client.Write(message);
                    TotalBytesSent += message.Length;
                }
            }
        }

        //client has connected to this device
        private void Listener_OnConnectedToClient(object sender, BluetoothClient e)
        {
            client = e;
            IsListerning = false;
            isHost = true;
            client.onDisconnect_Evt += Client_onDisconnect_Evt;
            client.onRecivedMessage_Evt += Client_onRecivedMessage_Evt;
            Send_Messages.Clear();
            NetworkCommands.Clear();
            ThreadSafeEvents.Clear();

            Connected = true;

            ThreadSafeEvents.Add("connected");
        }

        //this device has connected to another device
        private void Client_onConnected_Evt(object sender, EventArgs e)
        {
            isHost = false;
            Send_Messages.Clear();
            NetworkCommands.Clear();
            ThreadSafeEvents.Clear();

            Connected = true;


            ThreadSafeEvents.Add("connected");
        }

        //a message has been recived
        private void Client_onRecivedMessage_Evt(object sender, byte[] e)
        {
            ProccessMessage(e);
        }

        //disconnected
        private void Client_onDisconnect_Evt(object sender, EventArgs e)
        {
            if (isHost)
            {
                client.onDisconnect_Evt -= Client_onDisconnect_Evt;
                client.onRecivedMessage_Evt -= Client_onRecivedMessage_Evt;
            }
            else
            {
                client.onConnected_Evt -= Client_onConnected_Evt;
                client.onDisconnect_Evt -= Client_onDisconnect_Evt;
                client.onRecivedMessage_Evt -= Client_onRecivedMessage_Evt;
            }


            Connected = false;
            ThreadSafeEvents.Add("disconnected");
        }
        //retrurn device list
        public override Dictionary<string, string> GetDevices()
        {
            return BluetoothPlugin.GetDevices();
        }
        //close the current connection
        public override void CloseConnection()
        {
            if (isHost)
            {
                if (client.isConnected)
                {
                    client.Disconnect();
                }
                if (IsListerning)
                {
                    StopListerningForClient();
                }
            }else
            {
                if (client.isConnected)
                {
                    client.Disconnect();
                }
            }
            base.CloseConnection();
        }
        //listern for a connecting device to join
        public override void ListernForClient()
        {
            isHost = true;

            listener.StartListerning();
            base.ListernForClient();
        }
        //stop listerning
        public override void StopListerningForClient()
        {
            isHost = false;
            listener.StopListerning();
            base.StopListerningForClient();
        }

        //connect to device
        public override void ConnectToDevice(string address)
        {
            client = new BluetoothClient();
            client.onConnected_Evt += Client_onConnected_Evt;
            client.onDisconnect_Evt += Client_onDisconnect_Evt;
            client.onRecivedMessage_Evt += Client_onRecivedMessage_Evt;
            client.StartClient(address);

        }


    }

}
