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
    public class ParallelCompressionBase : CompressionBase
    {
        //incoming data / threashold = amount of chunks/threads
        const int ChunkSizeThreashold = 2500;
        public override byte[] Compress(string value)
        {
            byte[] bytesValue = Encoding.UTF8.GetBytes(value);
            byte ThreadCount = 1;
            if (bytesValue.Length > ChunkSizeThreashold)
            {
                ThreadCount = (byte)(bytesValue.Length / ChunkSizeThreashold);
            }
           
            // Divide the byte array between theads
            int sizeOfDataChunks = bytesValue.Length / ThreadCount;

            // get remainder of byte array for last thread
            int Remainder = bytesValue.Length % ThreadCount;

            // 2D array, 1 byte array for each thread
            byte[][] compressedDataChunks = new byte[ThreadCount][];
            ManualResetEvent[] threadDoneEvents = new ManualResetEvent[ThreadCount];


            // Create Parellel Compression threads -----------------------------------
            for (int i = 0; i < ThreadCount; i++)
            {
                int startIndex = i * sizeOfDataChunks;

                int count = (i < ThreadCount - 1) ? sizeOfDataChunks : sizeOfDataChunks + Remainder;

                threadDoneEvents[i] = new ManualResetEvent(false);

                int threadID = i;

                // Create a thread for each chunk
                ThreadPool.QueueUserWorkItem(state =>
                {
                    //compress and store result
                    compressedDataChunks[threadID] = CompressChunk(bytesValue, startIndex, count);
                   
                    //set done
                    threadDoneEvents[threadID].Set();
                },i);
            }
            //wait for threads to complete
            WaitHandle.WaitAll(threadDoneEvents);


            //total length of data
            int CompressedDataLength = 0;
            for (int i = 0; i < ThreadCount; i++)
            {
                //append chunk length + extra 2 bytes for length
                CompressedDataLength += compressedDataChunks[i].Length+2;

            }

            // join each data chunk together------------------------------------
            byte[] compressedData = new byte[1+CompressedDataLength];
            //thread count is required to know how many chunks must be decompressed
            compressedData[0] = ThreadCount;
            int offset = 1;

            for (int i = 0; i < ThreadCount; i++)
            {
                //insert chunk size at front of chunk in final array, for the decompresion sides knowledge

                BitConverter.GetBytes((ushort)compressedDataChunks[i].Length).CopyTo(compressedData,offset);
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
            byte ThreadCount = value[0];

            // 2D array, 1 byte array for each thread
            byte[][] DecompressedDataChunks = new byte[ThreadCount][];

            //offset for chunkdata
            int offset = 1;
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
                    threadDoneEvents[threadIndex].Set();
                });

                //move offset to next chunk
                offset += count;
            }


            //block main thread and wait for each thread to be done --------------------
            WaitHandle.WaitAll(threadDoneEvents);

            //total length of data
            int DeCompressedDataLength = 0;
            for (int i = 0; i < ThreadCount; i++)
            {
                //append chunk length
                DeCompressedDataLength += DecompressedDataChunks[i].Length;

            }


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