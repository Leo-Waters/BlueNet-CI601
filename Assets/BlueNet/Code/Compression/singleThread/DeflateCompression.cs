/*--------------------------------------------------------
 *  BlueNet A Bluetooth Multiplayers solution for unity.
 *  https://github.com/Leo-Waters/BlueNet-CI601
 * 
 *  Script: Deflate Compression for bluenet
 *  Created by leo waters
 *  University Contact Email: l.waters3@uni.brighton.ac.uk
 *  Personal Contact Email li0nleo117@gmail.com
 * -------------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;
namespace BlueNet.Compression
{
    public class DeflateCompressor : CompressionBase
    {

        public override byte[] Compress(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            using (var memoryStream = new MemoryStream())
            {
                using (var DeflateStream = new DeflateStream(memoryStream, System.IO.Compression.CompressionLevel.Fastest))
                {
                    DeflateStream.Write(bytes);
                }
                return memoryStream.ToArray();
            }

        }
        public override string DeCompress(byte[] value)
        {
            string output;
            using (var outputStream = new MemoryStream())
            {
                using (var compressStream = new MemoryStream(value))
                {
                    using (var deflateStream = new DeflateStream(compressStream, CompressionMode.Decompress))
                    {
                        deflateStream.CopyTo(outputStream);
                    }
                }

                output= Encoding.UTF8.GetString(outputStream.ToArray());
            }

            return output;
        }
    }
}