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

        //gets the messages to send to the other player as compressed bytes
        public byte[] GetMessage()
        {
            string data = "";

            while (!Send_Messages.IsEmpty)
            {

                if (Send_Messages.TryTake(out string message))
                {
                    data += message;
                }
            }

            if (!string.IsNullOrEmpty(data))
            {
                byte[] message = BlueNetManager.Compression.Compress(data);

                byte[] messageLength = BitConverter.GetBytes((ushort)message.Length);

                //append message length to front of message, this is to counteract buffer overflow on reciving end
                byte[] FinalMessage = new byte[message.Length + messageLength.Length];
                System.Buffer.BlockCopy(messageLength, 0, FinalMessage, 0, messageLength.Length);
                System.Buffer.BlockCopy(message, 0, FinalMessage, messageLength.Length, message.Length);
                return FinalMessage;
            }
            else
            {
                return null;
            }
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

        //the currently being read message
        byte[] IncomingMessageBuffer;
        ushort messagebufferindex=0;
        string DecompressedMessage="";

        //the length of the current message
        byte[] messageLength = new byte[2];
        byte messageLengthIndex=0;

        internal void ProccessMessage(byte[] message) // 5hello 3yes 3bob
        {

            DecompressedMessage = string.Empty;
            TotalBytesRead += message.Length;
            //first 2 bytes == ushort == length of incoming message
            //create buffer with length and buffer bytes until length is reached
            
            for (int i = 0; i < message.Length; i++)
            {
                //get incoming message length and create buffer for the incoming data
                if (IncomingMessageBuffer==null)
                {
                    messageLength[messageLengthIndex] = message[i];
                    if (messageLengthIndex == 1)
                    {
                        //Debug.Log("Creating buffer with size:" + BitConverter.ToUInt16(messageLength).ToString());
                        IncomingMessageBuffer = new byte[BitConverter.ToUInt16(messageLength)];
                        messageLengthIndex = 0;
                    }
                    else
                    {
                        messageLengthIndex++;
                    }
                    continue;
                }
                else 
                {
                    IncomingMessageBuffer[messagebufferindex] = message[i];
                    messagebufferindex++;
                    //reached end of buffer, get message and clean up
                    if (IncomingMessageBuffer.Length == messagebufferindex)
                    {
                        DecompressedMessage += BlueNetManager.Compression.DeCompress(IncomingMessageBuffer);
                        IncomingMessageBuffer = null;
                        messagebufferindex = 0;
                    }
                }


            }

            if (DecompressedMessage != string.Empty)
            {
                //Debug.Log("Incoming " + DecompressedMessage);

                for (int i = 0; i < DecompressedMessage.Length; i++)
                {
                    if (DecompressedMessage[i] == '|')
                    {//Reached end of command add to vector

                        CurrentArgs.Add(CurrentArg);//add arg tp list
                        CurrentArg = string.Empty;//clear added

                        //Debug.Log(CurrentArgs);
                        NetworkCommands.Add(new NetworkCommand(CurrentCommand, CurrentArgs.ToArray()));
                        CurrentCommand = "";
                        CurrentArgs.Clear();
                        GotCommand = false;
                    }
                    else
                    {//reading letter by letter
                        if (GotCommand == false)
                        {
                            if (DecompressedMessage[i] == ',')
                            { //got first arg being the command name
                                GotCommand = true;
                            }
                            else
                            {
                                CurrentCommand += DecompressedMessage[i];//append letters until , is reached
                            }

                        }
                        else if (DecompressedMessage[i] == ',')
                        {//got arg
                            CurrentArgs.Add(CurrentArg);//add arg tp list
                            CurrentArg = string.Empty;//clear added
                        }
                        else
                        {
                            CurrentArg += DecompressedMessage[i];//add chars
                        }
                    }
                }
            }

        }


    }

}
