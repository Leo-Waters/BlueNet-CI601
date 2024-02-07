using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;
namespace BlueNet.Compression
{
    public class GzipCompressor : CompressionBase
    {

        public override byte[] Compress(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, System.IO.Compression.CompressionLevel.Fastest))
                {
                    gzipStream.Write(bytes);
                }
                return memoryStream.ToArray();
            }

        }
        public override string DeCompress(byte[] value)
        {
            using (var memoryStream = new MemoryStream(value))
            {
                using (var decompressStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    using (var outputStream = new MemoryStream())
                    {
                        decompressStream.CopyTo(outputStream);
                        return Encoding.UTF8.GetString(outputStream.ToArray());
                    }
                    
                }
            }
        }
    }
}