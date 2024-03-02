/*--------------------------------------------------------
 *  BlueNet A Bluetooth Multiplayers solution for unity.
 *  https://github.com/Leo-Waters/BlueNet-CI601
 * 
 *  Script: Gzip Compression for bluenet
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
    public class GzipCompressor : CompressionBase
    {

        public override byte[] Compress(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, System.IO.Compression.CompressionLevel.Fastest,true))
                {
                    gzipStream.Write(bytes);
                }
                return memoryStream.ToArray();
            }

        }
        public override string DeCompress(byte[] value)
        {
            string Output;
#if UNITY_ANDROID && !UNITY_EDITOR
            //gizip dosnt work or andorid--
            try
            {
                
                using (var memoryStream = new MemoryStream(value))
                {
                    using (var decompressStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                    {
                        using (var outputStream = new MemoryStream())
                        {
                            decompressStream.CopyTo(outputStream);
                            Output= Encoding.UTF8.GetString(outputStream.ToArray());
                        }

                    }
                }
            }
            catch
            {
                string path = "/storage/emulated/0/Android/data/com.DefaultCompany.CI601BlueNet/files/failedToSendBytes.txt";
                File.WriteAllBytes(path, value);
                Debug.Log(value);
                Output= "error|";
            }
#else
            using (var memoryStream = new MemoryStream(value))
            {
                using (var decompressStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    using (var outputStream = new MemoryStream())
                    {
                        decompressStream.CopyTo(outputStream);
                        Output = Encoding.UTF8.GetString(outputStream.ToArray());
                    }

                }
            }
#endif
            return Output;
        }
    }
}