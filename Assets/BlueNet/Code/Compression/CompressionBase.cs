using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        deflate=2,
        customString=3,
        ParallelGzip=4,
        ParallelDefalte=5
    }

    public class CompressionBase
    {
#if UNITY_EDITOR
        public struct CompressionAlgorithmSpeedTest
        {
            public CompressionAlgorithmSpeedTest(string _name,double cTime,double dcTime,int reduction)
            {
                name = _name;
                compTime = cTime;
                decompTime = dcTime;
                sizeReduction = reduction;
            }
            public string name;
            public double TotalTime()
            {
                return compTime + decompTime;
            }
            public double compTime;
            public double decompTime;

            public int sizeReduction;
        }
        public CompressionAlgorithmSpeedTest Test(string message = "test,1,2,4,hello|test,1,2,4,hello|test,1,2,4,hello,testmessage|test,1,2,4,hello,testmessage|")
        {

            int sizeReduction = Encoding.UTF8.GetByteCount(message);
            Stopwatch sw = Stopwatch.StartNew();
            var data = Compress(message);
            sw.Stop();
            //UnityEngine.Debug.Log(Encoding.UTF8.GetString(data));
            double compSec = sw.Elapsed.TotalSeconds;


            sw = Stopwatch.StartNew();
            var rtn = DeCompress(data);
            
            sw.Stop();

            sizeReduction -= data.Length;
            double DecompSec = sw.Elapsed.TotalSeconds;

            string name = this.GetType().ToString();
            UnityEngine.Debug.Log(name + ": " + rtn);

            return new CompressionAlgorithmSpeedTest(name.Substring(name.LastIndexOf('.')), compSec*1000, DecompSec*1000, sizeReduction);
        }
#endif
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