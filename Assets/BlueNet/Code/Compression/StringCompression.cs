using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;
namespace BlueNet.Compression
{
    public class StringCompressor : CompressionBase
    {
        public void Test()
        {
            string message = "test,1,2,4,hello|test,1,2,4,hello|test,1,2,4,hello,testmessage|test,1,2,4,hello,testmessage|";
            Debug.Log(message);
            Debug.Log("before comp:" + Encoding.UTF8.GetByteCount(message).ToString());
            var data =Compress(message);

            Debug.Log(Encoding.UTF8.GetString(data));
            Debug.Log("After comp:" + data.Length);
            Debug.Log(DeCompress(data));

        }

        //number of duplicates must be higher than this threshold to be compressed
        //lower the threshold the more compressed data but also more computation resulting in latency
        const int DuplicateThreshold = 2;

        //the length the string must be to compress, too low of a threash hold results in added computation and potentials larger data due to key symbols added ^value^ ^keyindex^
        const int DuplicateStringLengthThreshold = 3;
        List<string> DuplicateCounter(string[] cmds)
        {
            //count duplicates of each argument
            var DuplicateCount = new Dictionary<string, int>();
            for (int c = 0; c < cmds.Length; c++)
            {
                var arguments = cmds[c].Split(',', System.StringSplitOptions.RemoveEmptyEntries);
                for (int a = 0; a < arguments.Length; a++)
                {
                    if(arguments[a].Length> DuplicateStringLengthThreshold)
                    {
                        if (DuplicateCount.TryGetValue(arguments[a], out int lastValue))
                        {
                            DuplicateCount[arguments[a]] = lastValue+1;
                        }
                        else
                        {
                            DuplicateCount.Add(arguments[a], 1);
                        }
                    }

                    
                }
            }
            //create list of valid duplicates for compression 

            var duplicates =new List<string>();
            foreach (var arg in DuplicateCount)
            {
                if (arg.Value > DuplicateThreshold)
                {
                    duplicates.Add(arg.Key);
                }

            }
            return duplicates;
        }


        public override byte[] Compress(string value)
        {
            var commands = value.Split('|',System.StringSplitOptions.RemoveEmptyEntries);

            List<string> Duplicates = DuplicateCounter(commands);

            var duplicateArgs = new Dictionary<string, string>();
            int KeyIndex = 0;
            for (int c = 0; c < commands.Length; c++)
            {
                var arguments = commands[c].Split(',', System.StringSplitOptions.RemoveEmptyEntries);
                for (int a = 0; a < arguments.Length; a++)
                {
                    //is the argument a duplicate
                    if (duplicateArgs.ContainsKey(arguments[a]))
                    {
                        //set argument as duplicate key
                        arguments[a] = duplicateArgs[arguments[a]];
                    }
                    else
                    {
                        //is the argument the first of a duplicate
                        if (Duplicates.Contains(arguments[a]))
                        {

                            //set value in map for following duplicates
                            duplicateArgs.Add(arguments[a], "^"+KeyIndex.ToString()+"^");
                            //set as duplicate key
                            arguments[a] = "^" + arguments[a] + "^";

                            KeyIndex++;
                        }
                    }
                }
                //replace command with compressed arguments
                commands[c] = string.Join(",", arguments);
            }

            //return all commands as byte array
            return Encoding.UTF8.GetBytes(string.Join("|",commands)+"|");
        }
        public override string DeCompress(byte[] value)
        {
            var commands = Encoding.UTF8.GetString(value).Split('|', System.StringSplitOptions.RemoveEmptyEntries);

            int KeyIndex = 0;
            var duplicateArgs = new Dictionary<string, string>();
            for (int c = 0; c < commands.Length; c++)
            {
                var arguments = commands[c].Split(',', System.StringSplitOptions.RemoveEmptyEntries);
                for (int a = 0; a < arguments.Length; a++)
                {
                    //is this a duplicate
                    if (duplicateArgs.TryGetValue(arguments[a],out string OgValue))
                    {
                        //set to origional value
                        arguments[a] = OgValue;
                    }
                    //is this a duplicate key
                    else if (arguments[a][0] == '^')
                    {
                        //extract correct value
                        string Value = arguments[a].Substring(1, arguments[a].Length-2);
                        arguments[a] = Value;
                        //add value to map for future keys 
                        duplicateArgs.Add("^" + KeyIndex + "^", Value);
                        KeyIndex++;
                    }
                }
                //replace command with compressed arguments
                commands[c] = string.Join(",", arguments);
            }

            //return incoming data as string
            return string.Join("|", commands)+"|";
        }
    }
}