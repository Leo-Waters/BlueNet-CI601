using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using System;
using BlueNet.DataTypes;
using UnityEngine.Android;
using BlueNet.Transports;
using BlueNet.Compression;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace BlueNet
{
    //Provides Functionality for Creating and joining bluetooth Sessions
    [AddComponentMenu("BlueNet/Network Manager")]
    public class BlueNetManager : MonoBehaviour
    {
        //instantiate a new network manager via context menu
#if UNITY_EDITOR
        [MenuItem("GameObject/BlueNet/NetworkManager")]
        private static void CreateButton(MenuCommand menuCommand)
        {
            var go = new GameObject("Network_Manager");
            go.AddComponent<BlueNetManager>();

            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
#endif
        private void OnApplicationQuit()
        {
            //clean up
            if (IsConnected())
            {
                CloseClient();
            }
            if (IsHost())
            {
                CloseServer();
            }
        }
        //the active manager object
        public static BlueNetManager Instance;

        //the current network transport in use
        static BlueNetTransportBase netTransport;

        //is this player connected
        public static bool IsConnected()
        {
            return netTransport.IsConnected();
        }
        //is this player the host
        public static bool IsHost()
        {
            return netTransport.IsHost();
        }

        //events-------------------------------------------------------------------------------------------------
        public static event EventHandler OnDisconnected;
        public static event EventHandler OnConnected;


        public CompressionAlgorithmType ActiveCompressionAlgorithm=CompressionAlgorithmType.none;
        public static CompressionBase Compression;
        //used to identify the application
        public int GUID = 1234;
        //scen you want to load on disconnect, -1 = none
        public int DisconectedSceneIndex = -1;

        private float p_ping;
        public float ping {
            get
            {
                return p_ping;
            }
        }

        //get paired device list, name and address key value pairs
        public static Dictionary<string, string> GetDevices()
        {
            return netTransport.GetDevices();
        }

        //join a server
        public static void StartClient(string deviceAddress)
        {
            netTransport.ConnectToDevice(deviceAddress);
            Debug.Log(Application.persistentDataPath);

        }
        //close client
        public static void CloseClient()
        {
            netTransport.CloseConnection();
        }

        //start listerning for a joining client
        public static void StartServer()
        {
            if (netTransport.IsConnected() || netTransport.isListerningForClient())
            {
                Debug.LogWarning("Start Server Was Called but is connected or listing for a client");
            }
            netTransport.ListernForClient();
        }

        //close the active connection or stop listerning for a client
        public static void CloseServer()
        {
            if (IsConnected())
            {
                netTransport.CloseConnection();
            }
            else
            {
                netTransport.StopListerningForClient();
            }
        }





        //initazlie NetworkingObjects-------------------------------------------------

        static Dictionary<int, BlueNetObject> NetworkObjects=new Dictionary<int, BlueNetObject>();

        //register a blue net object, called by bluenet object when connected
        public static bool RegisterNetworkObject(BlueNetObject Obj)
        {
            return NetworkObjects.TryAdd(Obj.NetworkID, Obj);
        }
        //remove bluenet object from register
        public static void RemoveNetworkObject(BlueNetObject Obj)
        {
            if(NetworkObjects.ContainsKey(Obj.NetworkID)){
                NetworkObjects.Remove(Obj.NetworkID);
            }
        }

        //send an rpc over the transport
        public static void SendRPC(NetworkCommand Command)
        {
            netTransport.SendCommand(Command,false);
        }
        //propergate a network update to its destination network object
        void UpdateNetworkObject(NetworkCommand Command)
        {
            
            if(NetworkObjects.ContainsKey(Command.GetInt(0))){
                NetworkObjects[Command.GetInt(0)].ReciveNetworkUpdate(Command);
            }
            else
            {
                Debug.LogWarning("Recived Update for NetObject " + Command.GetInt(0) + " but it dosnt exist");
            }
        }
        //propergate a RPC to its destination network object
        void PropergateRPC(NetworkCommand Command)
        {
            //Debug.Log("cmd" + Command.GetCommand() +" "+ Command.ToString());
            if (NetworkObjects.ContainsKey(Command.GetInt(0)))
            {
                
                NetworkObjects[Command.GetInt(0)].ReciveRPC(Command);
            }
            else
            {
                Debug.LogWarning("Recived RPC for NetObject " + Command.GetInt(0) + " but it dosnt exist");
            }
        }

        //initalize the network manager
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Another instance of the network manager tried to initalize however there can only be one active at a time");
                Destroy(this);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(Instance);

            //set the correct network transport base on compiled platform
#if UNITY_ANDROID && !UNITY_EDITOR
            //requrest permission to use bluetooth for android
            const string bluetooth_permission = "android.permission.BLUETOOTH";
            const string bluetoothAdmin_permission = "android.permission.BLUETOOTH_ADMIN";
            if (!Permission.HasUserAuthorizedPermission(bluetooth_permission) 
                ||
                !Permission.HasUserAuthorizedPermission(bluetoothAdmin_permission))
            {
                Permission.RequestUserPermission(bluetooth_permission);
                Permission.RequestUserPermission(bluetoothAdmin_permission);
            }
            netTransport = new BlueNetTransportAndroid(GUID);
#else
            netTransport = new BlueNetTransportWindows(GUID);
#endif

           
            SetCompressionAlgoithm(ActiveCompressionAlgorithm);
        }

        void SetCompressionAlgoithm(CompressionAlgorithmType type)
        {
            switch (type)
            {
                case CompressionAlgorithmType.gzip:
                    Compression = new GzipCompressor();
                    break;
                case CompressionAlgorithmType.deflate:
                    Compression = new DeflateCompressor();
                    break;
                case CompressionAlgorithmType.customString:
                    Compression = new StringCompressor();
                    break;
                case CompressionAlgorithmType.ParallelDefalte:
                    Compression = new ParallelDeflateCompressor();
                    break;
                case CompressionAlgorithmType.ParallelGzip:
                    Compression = new ParallelGzipCompressor();
                    break;
                case CompressionAlgorithmType.none:
                default:
                    Compression = new CompressionBase();
                    break;
            }
        }
        //time when the ping was sent
        float pingSendTime = 0;
        //time in seconds, in which the ping will be sent
        float PingSendPeriod = 2;
        //remainging time until next ping
        float PingDelay = 0;

        public static int ObjectUpdatesRecived=0;
        private void Update()
        {
            if (IsConnected())
            {
                PingDelay -= Time.deltaTime;
                if (PingDelay <= 0)
                {
                    pingSendTime = Time.time;
                    netTransport.SendCommand(new DataTypes.NetworkCommand("RequestPing"), false);
                    PingDelay = PingSendPeriod;
                }
            }

            var commands = netTransport.GetCommands();//proccesss recived Data
            foreach (var command in commands)
            {
                
                switch (command.GetCommand())
                {
                    case "SceneChange":
                        UnityEngine.SceneManagement.SceneManager.LoadScene(command.GetInt(0));
                        break;
                    case "ObjectUpdate":
                        UpdateNetworkObject(command);
                        ObjectUpdatesRecived++;
                        break;

                    case "ObjectRPC":
                        PropergateRPC(command);
                        ObjectUpdatesRecived++;
                        break;
                    case "RequestPing":
                        netTransport.SendCommand(new DataTypes.NetworkCommand("ReceivePing"), false);
                        break;
                    case "ReceivePing":
                        p_ping = Time.time - pingSendTime;
                        break;

                    default:
                        Debug.Log("Recived Command with no Proccess:" + command.GetCommand());
                        break;
                }
            }


            var events = netTransport.GetEvents(); //call events that are requested from other threads
            foreach(string Event in events)
            {
                if(Event== "connected")
                {
                    OnConnected?.Invoke(null, EventArgs.Empty);
                }
                else if(Event== "disconnected")
                {
                    if(DisconectedSceneIndex != -1)//return to menu
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene(DisconectedSceneIndex);
                    }
                    
                    OnDisconnected?.Invoke(null, EventArgs.Empty);
                }
            }

            //send latest messages 
            netTransport.Send();
        }



        //Network Functiions
        public static void ChangeScene(int BuildIndex)
        {
            netTransport.SendCommand(new DataTypes.NetworkCommand("SceneChange", BuildIndex.ToString()),true);
        }

    }
}