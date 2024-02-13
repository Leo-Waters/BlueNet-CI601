using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;
namespace BlueNet.Compression
{
    public class StringCompressor : CompressionBase
    {
        const int DuplicateStringLengthThreshold = 3;
        StringBuilder stringBuilder;
        StringBuilder current;
        public StringCompressor()
        {
            stringBuilder = new StringBuilder(500);
            current = new StringBuilder(50);
        }
        public override byte[] Compress(string value)
        {
            stringBuilder.Clear();

            //identified duplicates and the replacement
            var duplicateArgs = new Dictionary<string, string>();
            int KeyIndex = 0;
            //the current value being read
            StringBuilder current = new StringBuilder();
            for (int c = 0; c < value.Length; c++)
            {
                //is this the end of a value
                if (value[c] == ',' || value[c] == '|')
                {
                    //is the value large enough to compress
                    if (current.Length > DuplicateStringLengthThreshold)
                    {
                        string currentValue = current.ToString();
                        if (duplicateArgs.ContainsKey(currentValue))
                        {
                            //this value was alread a duplicate so replace with the key
                            stringBuilder.Append(duplicateArgs[currentValue]);
                        }
                        else
                        {
                            //this value is the first of its kind, set as a value 
                            stringBuilder.Append("^").Append(currentValue).Append("^");
                            duplicateArgs.Add(currentValue, "^" + KeyIndex.ToString() + "^");
                            KeyIndex++;
                        }
                    }
                    else
                    {
                        stringBuilder.Append(current);
                    }
                    current.Clear();
                    stringBuilder.Append(value[c]);
                }
                else
                {
                    current.Append(value[c]);
                }

            }

            //call base method to convert string to byte array
            return base.Compress(stringBuilder.ToString());
        }
        public override string DeCompress(byte[] bytesvalue)
        {
            //get string from byte value
            string value = base.DeCompress(bytesvalue);
            stringBuilder.Clear();

            //identified duplicates
            var duplicateArgs = new Dictionary<string, string>();
            int keyIndex = 0;
            current.Clear();
            bool gettingCompVal = false;
            for (int c = 0; c < value.Length; c++)
            {
                //getting a compressed value
                if (gettingCompVal)
                {
                    if (value[c] == '^')
                    {
                        gettingCompVal = false;

                        string currentValue = current.ToString();
                        current.Clear();
                        //is the duplicate value a key
                        if (duplicateArgs.TryGetValue(currentValue, out string key))
                        {
                            //replace value with correct value
                            stringBuilder.Append(key);
                        }
                        else
                        {
                            //is the value for the keys
                            stringBuilder.Append(currentValue);
                            //add to dictonary for keys to decode
                            duplicateArgs.Add(keyIndex.ToString(), currentValue);
                            keyIndex++;
                        }

                        continue;
                    }
                    else
                    {
                        current.Append(value[c]);
                    }
                }
                else
                {
                    //signifies a compressed value is about to be read
                    if (value[c] == '^')
                    {
                        gettingCompVal = true;
                        continue;
                    }
                    stringBuilder.Append(value[c]);
                }

            }
            //return message as a string
            return stringBuilder.ToString();
        }
    }
}