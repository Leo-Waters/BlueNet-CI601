using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using BlueNet.DataTypes;
namespace BlueNet.Transports
{
    //base class for inheritance, alowing for the addition of multiple transports for each platform
    public class BlueNetTransportBase
    {
        //init transport with a GUID to differentiate the application when joining
        public BlueNetTransportBase(int _GUID)
        {
            GUID = _GUID;
        }
        //varribles---------------------------------
        public static int TotalBytesRead;
        public static int TotalBytesSent;

        internal int GUID = 1234;

        internal bool Connected = false;
        internal bool IsListerning = false;
        internal bool isHost = false;

        //getters-----------------------------------

        //is this transport the host
        public bool IsHost()
        {
            return isHost;
        }

        //Returns a new GUID
        public System.Guid GetGuid()
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(GUID).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        //is the Transport connected to another device
        public bool IsConnected()
        {
            return Connected;
        }
        //is the Transport listerning for a joining client
        public bool isListerningForClient()
        {
            return IsListerning;
        }

        public virtual Dictionary<string, string> GetDevices() { return null; }

        public virtual void Send()
        {

        }

        //--------------------------------------------------------------------

        //listern for a device that wants to join
        public virtual void ListernForClient() { IsListerning = true; }
        //stop listerning for a joining player
        public virtual void StopListerningForClient() { IsListerning = false; }
        //connection to device
        public virtual void ConnectToDevice(string address){}

        //closes connection to client, call base after overide
        public virtual void CloseConnection()
        {
            isHost = false;
            Connected = false;
            ThreadSafeEvents.Add("Disconected");
        }

        //Commands sent localy or recived by the connected client are buffered here so that the manager can decode and execute them
        internal ConcurrentBag<NetworkCommand> NetworkCommands = new ConcurrentBag<NetworkCommand>();

        internal ConcurrentBag<string> ThreadSafeEvents = new ConcurrentBag<string>();

        //Messaged are buffered her via the main thread so that the network thread can send them
        internal ConcurrentBag<string> Send_Messages = new ConcurrentBag<string>();

        //returns all newly recived network commands
        public NetworkCommand[] GetCommands()
        {
            var ary = NetworkCommands.ToArray();
            NetworkCommands.Clear();
            return ary;
        }

        //returns events to dispatch
        public List<string> GetEvents()
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

        //Send a command to the other client with arguments
        public void SendCommand(NetworkCommand command,bool SendToSelf)
        {
            Send_Messages.Add(command.ToString());
            if (SendToSelf)
            {
                NetworkCommands.Add(command);
            }
            
        }

        string CurrentCommand = "";
        string CurrentArg = "";
        List<string> CurrentArgs = new List<string>();
        bool GotCommand = false;
        string OverFlow = "";

        internal void ProccessMessage(string message)
        {
            TotalBytesRead += Encoding.ASCII.GetByteCount(message);
            //Add OverFlow From Last Network Message
            string IncomingData = OverFlow + message;


            //Debug.Log("Incoming "+IncomingData);

            for (int i = 0; i < IncomingData.Length; i++)
            {
                if (IncomingData[i] == '|')
                {//Reached end of command add to vector

                    CurrentArgs.Add(CurrentArg);//add arg tp list
                    CurrentArg = string.Empty;//clear added

                    //Debug.Log(CurrentArgs);
                    NetworkCommands.Add(new NetworkCommand(CurrentCommand, CurrentArgs.ToArray()));
                    CurrentCommand = "";
                    CurrentArgs.Clear();
                    GotCommand = false;
                    OverFlow = string.Empty;
                }
                else
                {//reading letter by letter
                    if (GotCommand == false)
                    {
                        if (IncomingData[i] == ',')
                        { //got first arg being the command name
                            GotCommand = true;
                        }
                        else
                        {
                            CurrentCommand += IncomingData[i];//append letters until , is reached
                        }

                    }
                    else if (IncomingData[i] == ',')
                    {//got arg
                        CurrentArgs.Add(CurrentArg);//add arg tp list
                        CurrentArg = string.Empty;//clear added
                    }
                    else
                    {
                        CurrentArg += IncomingData[i];//add chars
                    }
                    OverFlow += IncomingData[i];
                }
            }
        }


    }

}
