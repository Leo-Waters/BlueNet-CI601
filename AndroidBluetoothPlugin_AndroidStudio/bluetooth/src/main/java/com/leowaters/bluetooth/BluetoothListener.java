package com.leowaters.bluetooth;

import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothServerSocket;
import android.bluetooth.BluetoothSocket;
import android.util.Log;

import java.io.IOException;
import java.util.UUID;

public class BluetoothListener {
    public BluetoothListenerCallbacks Callback;

    private AcceptThread acceptThread;
    private BluetoothDevice bluetoothDevice;

    public BluetoothListener(BluetoothListenerCallbacks callback)
    {
        Callback = callback;
    }

    public void StartListerning(){
        if(acceptThread==null){
            acceptThread=new AcceptThread();
            acceptThread.start();
        }

    }

    public void StopListerning(){
        if(acceptThread!=null){
            acceptThread.cancel();
            acceptThread=null;
        }
    }

    private class AcceptThread extends Thread{
        private  final BluetoothServerSocket serverSocket;

        public AcceptThread(){
            BluetoothServerSocket tmp=null;

            try {
                tmp =BluetoothPlugin.bluetoothAdapter.listenUsingInsecureRfcommWithServiceRecord(BluetoothPlugin.GetAppName(),BluetoothPlugin.GetUUID());
                Log.d(BluetoothPlugin.LOG_STR, "Listening Thread Created for "+BluetoothPlugin.GetAppName()+" on UUID: "+BluetoothPlugin.GetUUID());
            }catch (IOException e){
                Log.d(BluetoothPlugin.LOG_STR,"Failed to Create Listener for "+BluetoothPlugin.GetAppName()+" on UUID: "+BluetoothPlugin.GetUUID()+" Except: "+e.getMessage());
            }

            serverSocket =tmp;
        }
        @Override
        public void run(){
            Log.d(BluetoothPlugin.LOG_STR,"Listening Thread Running");
            BluetoothSocket socket=null;

            try {
                socket=serverSocket.accept();
                Log.d(BluetoothPlugin.LOG_STR,"Listening Thread, Accepted Connection");
            }catch (IOException e){
                Log.d(BluetoothPlugin.LOG_STR,"Failed to Create Listener for "+BluetoothPlugin.GetAppName()+" on UUID: "+BluetoothPlugin.GetUUID());
            }

            if(socket!=null){
                BluetoothClient client=new BluetoothClient(socket);
                Callback.OnConnected(client);

            }
        }

        public void cancel(){
            Log.d(BluetoothPlugin.LOG_STR,"Listening Thread Cancelled");
            try {
                serverSocket.close();
            }catch (IOException e){
                Log.d(BluetoothPlugin.LOG_STR,"Listening Thead close Failed: "+e.getMessage());
            }
        }
    }




}
