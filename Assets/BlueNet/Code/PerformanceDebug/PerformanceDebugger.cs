using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlueNet.Transports;
namespace BlueNet
{
    public static class PerformanceDebugger
    {
        //Testing Statistics--------------------------------

        //reset debug info and log from current time
        public static void ResetStatistics()
        {
            BlueNetTransportBase.TotalBytesRead = BlueNetTransportBase.TotalBytesSent = ObjectUpdates = 0;
            DebugTimeStart = Time.realtimeSinceStartup;
        }
        public static float DebugTimeStart;

        public static int ObjectUpdates;


        //returns the average object updates per second since last statistic reset
        public static float AverageObjectUpdatesPerSecond { get { return ObjectUpdates / Time.realtimeSinceStartup - DebugTimeStart; } }
        //returns the average bytes read per second since last statistic reset
        public static float AverageBytesReadPerSecond { get { return BlueNetTransportBase.TotalBytesRead / Time.realtimeSinceStartup - DebugTimeStart; } }
        //returns the average bytes sent per second since last statistic reset
        public static float AverageBytesSentPerSecond { get { return BlueNetTransportBase.TotalBytesSent / Time.realtimeSinceStartup - DebugTimeStart; } }


        //----------------------------------------------------------
    }
}