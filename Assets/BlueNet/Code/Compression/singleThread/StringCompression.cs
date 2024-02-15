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
        StringBuilder CompressStringBuilder;
        StringBuilder CompressCurrent;
        StringBuilder DeCompressStringBuilder;
        StringBuilder DeCompressCurrent;
        public StringCompressor()
        {
            CompressStringBuilder = new StringBuilder(500);
            CompressCurrent = new StringBuilder(50);
            DeCompressStringBuilder = new StringBuilder(500);
            DeCompressCurrent = new StringBuilder(50);
        }
        public override byte[] Compress(string value)
        {
            CompressStringBuilder.Clear();

            //identified duplicates and the replacement
            var duplicateArgs = new Dictionary<string, string>();
            int KeyIndex = 0;
            //the current value being read
            CompressCurrent.Clear();
            for (int c = 0; c < value.Length; c++)
            {
                //is this the end of a value
                if (value[c] == ',' || value[c] == '|')
                {
                    //is the value large enough to compress
                    if (CompressCurrent.Length > DuplicateStringLengthThreshold)
                    {
                        string currentValue = CompressCurrent.ToString();
                        if (duplicateArgs.ContainsKey(currentValue))
                        {
                            //this value was alread a duplicate so replace with the key
                            CompressStringBuilder.Append(duplicateArgs[currentValue]);
                        }
                        else
                        {
                            //this value is the first of its kind, set as a value 
                            CompressStringBuilder.Append("^").Append(currentValue).Append("^");
                            duplicateArgs.Add(currentValue, "^" + KeyIndex.ToString() + "^");
                            KeyIndex++;
                        }
                    }
                    else
                    {
                        CompressStringBuilder.Append(CompressCurrent);
                    }
                    CompressCurrent.Clear();
                    CompressStringBuilder.Append(value[c]);
                }
                else
                {
                    CompressCurrent.Append(value[c]);
                }

            }

            //call base method to convert string to byte array
            return base.Compress(CompressStringBuilder.ToString());
        }
        public override string DeCompress(byte[] bytesvalue)
        {
            //get string from byte value
            string value = base.DeCompress(bytesvalue);
            DeCompressStringBuilder.Clear();

            //identified duplicates
            var duplicateArgs = new Dictionary<string, string>();
            int keyIndex = 0;
            DeCompressCurrent.Clear();
            bool gettingCompVal = false;
            for (int c = 0; c < value.Length; c++)
            {
                //getting a compressed value
                if (gettingCompVal)
                {
                    if (value[c] == '^')
                    {
                        gettingCompVal = false;

                        string currentValue = DeCompressCurrent.ToString();
                        DeCompressCurrent.Clear();
                        //is the duplicate value a key
                        if (duplicateArgs.TryGetValue(currentValue, out string key))
                        {
                            //replace value with correct value
                            DeCompressStringBuilder.Append(key);
                        }
                        else
                        {
                            //is the value for the keys
                            DeCompressStringBuilder.Append(currentValue);
                            //add to dictonary for keys to decode
                            duplicateArgs.Add(keyIndex.ToString(), currentValue);
                            keyIndex++;
                        }

                        continue;
                    }
                    else
                    {
                        DeCompressCurrent.Append(value[c]);
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
                    DeCompressStringBuilder.Append(value[c]);
                }

            }
            //return message as a string
            return DeCompressStringBuilder.ToString();
        }
    }
}