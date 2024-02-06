using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BlueNet.DataTypes
{

    public class NetworkCommand
    {
        string Command;
        string[] Args;

        //Returns true if the NetCommand has Arguments
        public bool HasArgs()
        {
            if (Args == null)
            {
                return false;
            }
            else
            {
                return Args.Length != 0;
            }

        }

        

        //converts The Command to a string to send over the network
        public override string ToString()
        {
            string message = Command;

            if (HasArgs())
            {
                for (int i = 0; i < Args.Length; i++)
                {
                    message += "," + (Args[i]);
                }
            }

            return (message + "|");
        }

        public string GetCommand()
        {
            return Command;
        }
        public string[] GetArgs()
        {
            return Args;
        }

        public string GetString(int Index)
        {
            return Args[Index];
        }

        public int GetInt(int Index)
        {
            return int.Parse(Args[Index]);
        }

        public float GetFloat(int Index)
        {
            return float.Parse(Args[Index]);
        }

        //creates a network command with or without Args
        public NetworkCommand(string _Command, params string[] _Args)
        {

            Args = _Args;
            Command = _Command;
            
        }

        public NetworkCommand()
        {

        }
        public void SetData(string _Command, string[] _Args)
        {
            Args = _Args;
            Command = _Command;
        }
    }
}
