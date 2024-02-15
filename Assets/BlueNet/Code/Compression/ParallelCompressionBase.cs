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
    public class ParellelCompressionBase : CompressionBase
    {
        const int ThreadCount = 4;
        public override byte[] Compress(string value)
        {
            byte[] bytesValue = Encoding.UTF8.GetBytes(value);

            // Divide the byte array between theads
            int sizeOfDataChunks = bytesValue.Length / ThreadCount;

            // get remainder of byte array for last thread
            int Remainder = bytesValue.Length % ThreadCount;

            // 2D array, 1 byte array for each thread
            byte[][] compressedDataChunks = new byte[ThreadCount][];
            ManualResetEvent[] threadDoneEvents = new ManualResetEvent[ThreadCount];
            //total length of data
            int CompressedDataLength = 0;

            // Create Parellel Compression threads -----------------------------------
            for (int i = 0; i < ThreadCount; i++)
            {
                int startIndex = i * sizeOfDataChunks;

                int count = (i < ThreadCount - 1) ? sizeOfDataChunks : sizeOfDataChunks + Remainder;

                threadDoneEvents[i] = new ManualResetEvent(false);

                // Create a thread for each chunk
                ThreadPool.QueueUserWorkItem(ID =>
                {
                    int threadID = (int)ID;
                    //compress and store result
                    compressedDataChunks[(int)threadID] = CompressChunk(bytesValue, startIndex, count);
                    
                    //append chunk length + extra 2 bytes for length
                    CompressedDataLength += compressedDataChunks[(int)threadID].Length + 2;
                    //set done
                    threadDoneEvents[threadID].Set();
                },i);
            }
            //wait for threads to complete
            WaitHandle.WaitAll(threadDoneEvents);


            // join each data chunk together------------------------------------
            byte[] compressedData = new byte[CompressedDataLength];
            int offset = 0;

            for (int i = 0; i < ThreadCount; i++)
            {
                //insert chunk size at front of chunk in final array, for the decompresion sides knowledge
                BitConverter.GetBytes((ushort)compressedDataChunks[i].Length-2).CopyTo(compressedData,offset);
                offset+=2;
                //copy chunk to final array
                Array.Copy(compressedDataChunks[i], 0, compressedData, offset, compressedDataChunks[i].Length);

                offset += compressedDataChunks[i].Length;
            }

            return compressedData;

        }

        //compress a chunk of data
        public virtual byte[] CompressChunk(byte[] data, int start, int count)
        {
            //memory stream to write compression stream into
            using (var memoryStream = new MemoryStream())
            {
                //write data into stream
                memoryStream.Write(data, start, count);
                return memoryStream.ToArray();
            }
        }

        public override string DeCompress(byte[] value)
        {
            // 2D array, 1 byte array for each thread
            byte[][] DecompressedDataChunks = new byte[ThreadCount][];

            //total decomp size
            int DeCompressedDataLength = 0;

            //offset for chunkdata
            int offset = 0;
            // Create Parellel DeCompression threads -----------------------------------
            ManualResetEvent[] threadDoneEvents = new ManualResetEvent[ThreadCount];

            for (int i = 0; i < ThreadCount; i++)
            {
                //get the count for this chunk
                ushort count = BitConverter.ToUInt16(value, offset);
                offset += 2;

                int threadOffset = offset;
                int threadCount = count;
                int threadIndex = i;

                threadDoneEvents[i] = new ManualResetEvent(false);

                // Create a thread for each chunk

                ThreadPool.QueueUserWorkItem(state =>
                {
                    DecompressedDataChunks[threadIndex] = DeCompressChunk(value, threadOffset, threadCount);
                    DeCompressedDataLength += DecompressedDataChunks[threadIndex].Length;
                    threadDoneEvents[threadIndex].Set();
                });

                //move offset to next chunk
                offset += count + 2;
            }


            //block main thread and wait for each thread to be done --------------------
            WaitHandle.WaitAll(threadDoneEvents);


            // join each data chunk together------------------------------------
            byte[] DecompressedData = new byte[DeCompressedDataLength];
            offset = 0;
            for (int i = 0; i < ThreadCount; i++)
            {
                //copy chunk to final array
                Array.Copy(DecompressedDataChunks[i], 0, DecompressedData, offset, DecompressedDataChunks[i].Length);

                offset += DecompressedDataChunks[i].Length;
            }



            return Encoding.UTF8.GetString(DecompressedData);
        }

        //De compress a chunk of data
        public virtual byte[] DeCompressChunk(byte[] data, int start, int count)
        {
            byte[] chunk = new byte[count];
            Array.Copy(data, start, chunk, 0, count);
            return chunk;

        }


    }
}