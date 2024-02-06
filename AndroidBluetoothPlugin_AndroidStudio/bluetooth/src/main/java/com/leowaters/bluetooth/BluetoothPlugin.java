package com.leowaters.bluetooth;

import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothManager;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.util.Log;
import android.widget.Toast;

import java.util.LinkedList;
import java.util.List;
import java.util.UUID;

public class BluetoothPlugin {
    private static final BluetoothPlugin ourInstance = new BluetoothPlugin();
    public static final String LOG_STR="Leo waters - Bluetooth Plugin :";
    public static BluetoothPlugin getInstance() {
        return ourInstance;
    }

    public  static  BluetoothAdapter bluetoothAdapter;
    private BluetoothPlugin() {
        Log.i(LOG_STR,"Created Plugin Instance");
    }

    private static final BroadcastReceiver BluetoothStateChanged=new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            String action = intent.getAction();
            if(action.equals(BluetoothAdapter.ACTION_STATE_CHANGED)){
                final  int state=intent.getIntExtra((BluetoothAdapter.EXTRA_STATE),bluetoothAdapter.ERROR);

                switch (state){
                    case BluetoothAdapter.STATE_OFF:
                        Log.i(LOG_STR,"bluetoothAdapter Turned OFF");
                        break;
                    case BluetoothAdapter.STATE_ON:
                        Log.i(LOG_STR,"bluetoothAdapter Turned ON");
                        break;
                }
            }
        }
    };

    private static UUID uuid;
    private static String appName;
    public static UUID GetUUID(){
        return uuid;
    }
    public static String GetAppName(){
        return appName;
    }

    private static Activity unityActivity;
    //initialize the BluetoothPlugin
    public static void Init(Activity activity,String _appName,String _uuid_Str){
        unityActivity= activity;
        uuid=UUID.fromString(_uuid_Str);
        appName=_appName;

        BluetoothManager bluetoothManager = activity.getSystemService(BluetoothManager.class);
        bluetoothAdapter = bluetoothManager.getAdapter();
        if (bluetoothAdapter == null) {
            Log.i(LOG_STR,"Device does not have a bluetooth Adapter");
        }

        if (!bluetoothAdapter.isEnabled()) {
            Log.i(LOG_STR,"Attempting to enable bluetooth adapter");
            Intent enableBtIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
            activity.startActivity(enableBtIntent);

            IntentFilter intent=new IntentFilter(BluetoothAdapter.ACTION_STATE_CHANGED);
            activity.registerReceiver(BluetoothStateChanged,intent);

        }
    }

    public static BluetoothDevice GetDeviceFromAddress(String Address){
        for (BluetoothDevice device:
                bluetoothAdapter.getBondedDevices()) {

            if(device.getAddress().equals(Address)){
                return  device;
            }

        }
        Log.i(LOG_STR,"Failed to get device with address:"+Address);
        return null;
    }

    public static String[] GetPairedDevices(){
        List<String> Devices=new LinkedList<String>();
        for (BluetoothDevice device:
                bluetoothAdapter.getBondedDevices()) {
            Devices.add(device.getName());
            Devices.add((device.getAddress()));
        }
        return Devices.toArray(new String[Devices.size()]);
    }

    //Clean up the bluetoothPlugin
    public static void Dispose(){
        Log.i(LOG_STR,"Disposed plugin");
        unityActivity.unregisterReceiver(BluetoothStateChanged);
    }

    //displays a message on the screen
    public static void PerformToast(String Message){
        int duration = Toast.LENGTH_LONG;

        Toast toast = Toast.makeText(unityActivity, Message, duration);
        toast.show();
    }
}
