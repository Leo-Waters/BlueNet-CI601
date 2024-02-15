using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using UnityEngine;
namespace BlueNet.Compression
{
    public class ParallelDeflateCompressor : ParallelCompressionBase
    {
        //compress a chunk of data
        public override byte[] CompressChunk(byte[] data, int start, int count)
        {

            //memory stream to write compression stream into
            using (var memoryStream = new MemoryStream())
            {
                //create deflate stream
                using (var DeflateStream = new DeflateStream(memoryStream, System.IO.Compression.CompressionLevel.Fastest, true))
                {

                    //write data into stream
                    DeflateStream.Write(data, start, count);


                }
                return memoryStream.ToArray();
            }
        }

        //De compress a chunk of data
        public override byte[] DeCompressChunk(byte[] data, int start, int count)
        {

            //memory stream to write compression stream into
            using (var outputStream = new MemoryStream())
            {
                using (var compressStream = new MemoryStream(data,start, count))
                {
                    //create deflate stream
                    using (var deflateStream = new DeflateStream(compressStream, CompressionMode.Decompress))
                    {
                        //write data into stream
                        deflateStream.CopyTo(outputStream);
                    }
                }
                //return written data from stream as array
                return outputStream.ToArray();

            }

        }


    }
}