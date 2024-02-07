package com.leowaters.bluetooth;

import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;
import android.util.Log;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.Arrays;

public class BluetoothClient {

    public BluetoothClientCallback Callback;

    private ConnectThread connectThread;

    private NetworkThread networkThread;

    // The constructor takes a reference to the C# class
    // that implements the JavaCallback interface
    public BluetoothClient(BluetoothClientCallback callback)
    {
        Callback = callback;
    }
    BluetoothSocket socket;
    public BluetoothClient(BluetoothSocket _socket){socket=_socket;}

    public void SetCallback(BluetoothClientCallback callback){
        Callback = callback;
        connected(socket);
    }

    public void Write(byte[] data)
    {
        networkThread.write(data);
    }

    public void StartClient(String deviceAddress){

        BluetoothDevice device=BluetoothPlugin.GetDeviceFromAddress(deviceAddress);

        if(device==null){
            Log.d(BluetoothPlugin.LOG_STR,"Failed to find device with address:"+deviceAddress);
            return;
        }

        if (connectThread != null) {
            connectThread.cancel();
            connectThread=null;
        }
        connectThread=new ConnectThread(device);
        connectThread.start();
    }

    public void Disconnect(){
        if (connectThread != null) {
            connectThread.cancel();
            connectThread=null;
        }
        if (networkThread != null) {
            networkThread.cancel();
            networkThread=null;
        }
    }
    private void connected(BluetoothSocket socket){
        Log.d(BluetoothPlugin.LOG_STR,"Connected to device");
        networkThread=new NetworkThread(socket);
        networkThread.start();

        Callback.OnConnected();
    }


    private class NetworkThread extends Thread{
        private final BluetoothSocket socket;
        private final InputStream inputStream;
        private final OutputStream outputStream;
        public NetworkThread(BluetoothSocket _socket){
            Log.d(BluetoothPlugin.LOG_STR,"Network Thread Start");

            socket=_socket;
            InputStream tmpIn=null;
            OutputStream tmpOut=null;

            try {
                tmpIn=socket.getInputStream();
            } catch (IOException e) {
                e.printStackTrace();
            }
            try{
                tmpOut= socket.getOutputStream();
            } catch (IOException e) {
                e.printStackTrace();
            }
            inputStream=tmpIn;
            outputStream=tmpOut;
        }

        @Override
        public void run() {
            byte[] buffer= new byte[1024];
            int bytesRead;

            while (true){
                try {
                    bytesRead=inputStream.read(buffer);
                    //send received bytes to c# code via callback
                    Callback.OnRecivedMessage(Arrays.copyOf(buffer, bytesRead));
                } catch (IOException e) {
                    e.printStackTrace();
                    break;
                }

            }

            Callback.OnDisconnect();
        }

        public void write(byte[] data){
            try {
                outputStream.write(data);
            }catch (IOException e){
                e.printStackTrace();
            }
        }

        public void cancel(){
            try{
                socket.close();
            }catch (IOException e){
                e.printStackTrace();
            }
            Callback.OnDisconnect();
        }
    }

    private class ConnectThread extends Thread{
        private BluetoothSocket socket;
        private BluetoothDevice device;
        public ConnectThread (BluetoothDevice _device){
            Log.d(BluetoothPlugin.LOG_STR,"Connect Thread Created");
            device=_device;
        }
        @Override
        public void run(){
            BluetoothSocket tmp=null;
            Log.d(BluetoothPlugin.LOG_STR,"Connect Thread running");

            try {
                tmp = device.createRfcommSocketToServiceRecord(BluetoothPlugin.GetUUID());
                Log.d(BluetoothPlugin.LOG_STR,"Created socket");
            }catch (IOException e){
                Log.d(BluetoothPlugin.LOG_STR,"Failed to create socket: "+e.getMessage());
            }
            socket=tmp;

            try {
                socket.connect();
                Log.d(BluetoothPlugin.LOG_STR,"Connected Successfully");
            }catch (IOException e){
                Log.d(BluetoothPlugin.LOG_STR,"Failed to connect: "+e.getMessage());
                try {
                    socket.close();
                    Log.d(BluetoothPlugin.LOG_STR,"Closed Socket");
                }catch (IOException ee) {
                    Log.d(BluetoothPlugin.LOG_STR,"Failed to close Socket"+ee.getMessage());
                }

                Log.d(BluetoothPlugin.LOG_STR,"could not connect to UUID+"+BluetoothPlugin.GetUUID());
            }

            connected(socket);
        }

        public void cancel(){
            Log.d(BluetoothPlugin.LOG_STR,"Connect Thread Cancelled");
            try {
                socket.close();
            }catch (IOException e){
                Log.d(BluetoothPlugin.LOG_STR,"Connect Thread close Failed: "+e.getMessage());
            }


        }
    }
}
