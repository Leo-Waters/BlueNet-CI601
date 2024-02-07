using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BlueNet.Compression
{
    //todo
    //prefix message with length to counter overflow
    //re-write process message in transport base to deal with this incoming change
    public enum CompressionAlgorithmType
    {
        none=0,
        gzip=1,
        deflate=2
    }

    public class CompressionBase
    {

        public virtual byte[] Compress(string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }
        public virtual string DeCompress(byte[] value)
        {
            return Encoding.UTF8.GetString(value);
        }
    }
}