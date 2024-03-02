/*--------------------------------------------------------
 *  BlueNet A Bluetooth Multiplayers solution for unity.
 *  https://github.com/Leo-Waters/BlueNet-CI601
 * 
 *  Script: Performance debugger, stores info about netowkr performance
 *  Created by leo waters
 *  University Contact Email: l.waters3@uni.brighton.ac.uk
 *  Personal Contact Email li0nleo117@gmail.com
 * -------------------------------------------------------
 */

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
            BlueNetTransportBase.TotalBytesRead = BlueNetTransportBase.TotalBytesSent= BlueNetManager.ObjectUpdatesRecived= 0;
            BytesReadLast = BytesSentLast = ObjectUpdatesLast = 0;
        }
        public static int TotalObjectUpdates{get{return BlueNetManager.ObjectUpdatesRecived;}}
        public static int TotalBytesSent { get { return BlueNetTransportBase.TotalBytesSent; } }
        public static int TotalBytesRead { get { return BlueNetTransportBase.TotalBytesRead; } }


        //------get change functions, used for logging data-----

        private static int BytesReadLast, BytesSentLast, ObjectUpdatesLast;
        //returns how many bytes have been read since the last time this function was called
        public static int TotalBytesReadSinceLastCheck()
        {
            int Diff = TotalBytesRead - BytesReadLast;
            BytesReadLast = TotalBytesRead;
            return Diff;
        }
        //returns how many bytes have been seny since the last time this function was called
        public static int TotalBytesSentSinceLastCheck()
        {
            int Diff = TotalBytesSent - BytesSentLast;
            BytesSentLast = TotalBytesSent;
            return Diff;
        }
        //returns how many object updates have been executed since the last time this function was called
        public static int TotalObjectUpdatesSinceLastCheck()
        {
            int Diff = TotalObjectUpdates - ObjectUpdatesLast;
            ObjectUpdatesLast = TotalObjectUpdates;
            return Diff;
        }
    }
}