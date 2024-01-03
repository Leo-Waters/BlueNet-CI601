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
namespace BlueNet
{
    public static class BlueNetTransport
    {
        //init transport with a GUID to differentiate the application when joining
        public static void INIT(int _GUID)
        {
            GUID = _GUID;
            client = new BluetoothClient();
        }
        //varribles---------------------------------
        static int GUID = 1234;

        internal static bool IsHost()
        {
            return isHost;
        }

        static BluetoothClient client;
        static BluetoothListener bluetoothListener;

        static bool Connected = false;
        static bool IsListerning = false;
        static bool isHost = false;


        static Thread AcceptClientThread, NetworkWriteThread, NetworkReadThread;

        //getters----------------------------        

        //Returns a new GUID
        static System.Guid GetGuid()
        {
            //creates a guid from int for simplicity
            return BluetoothService.CreateBluetoothUuid(GUID);
        }

        //returns the Currently used BluetoothClient
        public static BluetoothClient GetClient()
        {
            return client;
        }

        //is the Transport connected to another device
        public static bool IsConnected()
        {
            return Connected;
        }
        //is the Transport listerning for a joining client
        public static bool isListerningForClient()
        {
            return IsListerning;
        }

        //connect to device
        public static void ConnectToDevice(BluetoothAddress address)
        {
            client = new BluetoothClient();
            client.Connect(address, GetGuid());

            if (client.Connected)
            {
                Connected = true;
                StartReadAndWriteThreads();
                
            }
            else
            {
                Debug.Log("Failed To Connect");
                client.Close();
            }


        }


        //start thread waiting for client
        public static void ListernForClient()
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
        public static void StopListerningForClient()
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
        static void AcceptClient()
        {
            client = bluetoothListener.AcceptBluetoothClient();
            Connected = true;
            bluetoothListener.Stop();
            StartReadAndWriteThreads();
        }
        //closes connection to client
        public static void CloseConnection()
        {
            isHost = false;
            client.Close();
            Connected = false;
            ThreadSafeEvents.Add("Disconected");
        }

        static ConcurrentBag<string> ThreadSafeEvents = new ConcurrentBag<string>();

        //Messaged are buffered her via the main thread so that the network thread can send them
        static ConcurrentBag<string> Send_Messages = new ConcurrentBag<string>();

        static ConcurrentBag<string> Recived_Messages = new ConcurrentBag<string>();

        public static void SendMessage(string message)
        {
            Debug.Log("Sending Message: " + message);
            Send_Messages.Add(message);
        }

        public static List<string> GetMessages()
        {
            List<string> messages = new List<string>();
            while (!Recived_Messages.IsEmpty)
            {
                string message;
                if (Recived_Messages.TryTake(out message))
                {
                    if (!string.IsNullOrWhiteSpace(message.Trim()))
                    {
                        messages.Add(message);
                    }

                }
            }

            return messages;
        }
        public static List<string> GetEvents()
        {
            List<string> Events = new List<string>();
            while (!ThreadSafeEvents.IsEmpty)
            {
                string message;
                if (ThreadSafeEvents.TryTake(out message))
                {
                    if (!string.IsNullOrWhiteSpace(message.Trim()))
                    {
                        Debug.Log("Event: " + message);

                        Events.Add(message);
                    }

                }
            }

            return Events;
        }

        
        //start read and write background threads for data transmision
        static void StartReadAndWriteThreads()
        {
            //clean up old data
            Send_Messages.Clear();
            Recived_Messages.Clear();
            ThreadSafeEvents.Clear();

            ThreadSafeEvents.Add("connected");
            NetworkReadThread = new Thread(ReadThread);
            NetworkWriteThread = new Thread(WriteThread);
           
            NetworkReadThread.Start();
            NetworkWriteThread.Start();
        }

        //reads incoming data from client
        static void ReadThread()
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
                    //Add OverFlow From Last Network Message
                    string IncomingData = Encoding.ASCII.GetString(ReadBuffer, 0, bytesRead);

                    Recived_Messages.Add(IncomingData);

                }       
            }
            if (stream != null)//client might have been disposed, clearing the stream already
            {
                stream.Close();
            }
        }

        //send Messages to client from Message buffer
        static void WriteThread()
        {
            var stream = client.GetStream();
            while (client.Connected && Connected && stream != null)
            {
                if (stream.CanWrite)
                {
                    string Data = "";

                    while (!Send_Messages.IsEmpty)
                    {
                        string message;
                        if(Send_Messages.TryTake(out message))
                        {
                            Data += message;
                        }
                    }

                    if (!string.IsNullOrEmpty(Data))
                    {
                        var buffer = System.Text.Encoding.UTF8.GetBytes(Data);
                        stream.Write(buffer, 0, buffer.Length);
                    }

                }

            }
            if (stream != null)//client could have been disposed, clearing the stream already
            {
                stream.Close();
            }

        }
    }

}
