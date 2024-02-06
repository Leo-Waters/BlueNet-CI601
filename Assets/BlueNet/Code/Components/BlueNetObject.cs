using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlueNet.DataTypes;
using System;

namespace BlueNet
{
    //Provides an intermediate layer for sending or reciving data and RPCS on a game object
    public class BlueNetObject : MonoBehaviour
    {
        [Header("NetID must be unique")]
        public int NetworkID = 0;

        [Header("False if client has control")]
        public bool HostIsOwner = true;

        private bool p_IsLocalyOwned = false;
        public bool IsLocalyOwned { get { return p_IsLocalyOwned; } }

        //behaviours to disable if not local player, eg camera, audio etc
        public Behaviour[] LocalPlayerOnlyBehaviours;

       
        //automaticaly syncronized varibles
        [Header("SyncVars"),SerializeField]
        List<SyncVar> SyncVars = new List<SyncVar>();

        //how oftern should sync var changes be checked and updates sent
        [Range(0.5f,2.5f)]
        public float VaribleSyncPeriod = 1;
        float syncDelay = 0;

        private void FixedUpdate()
        {
            if (IsLocalyOwned)
            {
                //timer to send updates of syncvars
                syncDelay += Time.fixedDeltaTime;
                if (syncDelay >= VaribleSyncPeriod)
                {
                    syncDelay = 0;
                    if (SyncVarChanged)
                    {
                        SendDataUpdate();
                    }
                }
            }
        }

        bool SyncVarChanged=false;

        //get a sync var via its tag
        SyncVar getVarByTag(string tag)
        {
            foreach (var item in SyncVars)
            {
                if (item.CompareTag(tag))
                {
                    return item;
                }
            }
            Debug.LogError("No Sync Var with tag: " + tag);
            return null;
        }
        //used to update the value internaly on network updates, call set var instead to update on other client
        void UpdateSyncVarValue(string tag,string value)
        {
            foreach (var item in SyncVars)
            {
                if (item.CompareTag(tag))
                {
                    item.NetworkUpdate(value);
                    OnSyncVarUpdate.Invoke(this, item);
                    return;
                }
            }
            Debug.LogError("No Sync Var with tag: " + tag);
        }
        //set the value of a sync var with string 
        public void SetVar(string tag,string value)
        {
            var syncvar = getVarByTag(tag);
            syncvar.Update(value);
            SyncVarChanged = true;

            OnSyncVarUpdate.Invoke(this, syncvar);
        }
        //set the value of a sync var with int 
        public void SetVar(string tag, int value)
        {
            var syncvar = getVarByTag(tag);
            syncvar.Update(value);
            SyncVarChanged = true;

            OnSyncVarUpdate.Invoke(this, syncvar);
        }
        //event called everytime a SyncVar is updated
        public event EventHandler<SyncVar> OnSyncVarUpdate;

        //used by manager to recive and update sync varibles 
        public void ReciveNetworkUpdate(NetworkCommand networkCommand)
        {
            Debug.LogWarning(networkCommand.ToString());
            string[] args = networkCommand.GetArgs();
            for (int i = 1; i < args.Length; i++)
            {
                string[] tagAndValue = args[i].Split("*");
                UpdateSyncVarValue(tagAndValue[0], tagAndValue[1]);
                
            }
        }

        //send all updated syncvaribles to the other player
        public void SendDataUpdate()
        {
            NetworkCommand command = new NetworkCommand();
            List<string> data = new List<string>();

            //ID of this object
            data.Add(NetworkID.ToString());

            foreach (var var in SyncVars)
            {
                //has the value changed and should it update
                if (var.ShouldSendUpdate())
                {
                    //adda sync var to parameters of the command
                    data.Add(var.getTag + '*' + var.GetString());
                    //tell the varrible its data it will be updated on the other player
                    var.SentUpdate();
                }
            }
            //data will be sent so the vars are up to date
            SyncVarChanged = false;

            command.SetData("ObjectUpdate", data.ToArray());

            BlueNetManager.SendRPC(command);
        }

#if UNITY_EDITOR
        //used in editor for checking if the ID is avalible
        bool ID_Exists(int ID)
        {
            var netobs = FindObjectsOfType<BlueNetObject>();

            foreach (var item in netobs)
            {
                if (ID == item.NetworkID && item != this)
                {
                    return true;
                }
            }

            return false;

        }
#endif


        private void OnValidate()
        {
#if UNITY_EDITOR
            //ensure the objects NetID is unique

            bool doDuplicatesExist = ID_Exists(NetworkID);

            if (doDuplicatesExist)
            {
                var netobs = FindObjectsOfType<BlueNetObject>();
                int newID = -1;

                for (int i = 0; i < netobs.Length; i++)
                {
                    if (ID_Exists(i) == false)
                    {
                        newID = i;
                        break;
                    }
                }

                if (newID == -1)
                {
                    newID = netobs.Length;
                }
                NetworkID = newID;
                Debug.Log("Assigned NetObject ID:" + newID);
            }      
#endif
        }


        private void Awake()
        {
            BlueNetManager.OnConnected += BlueNetManager_OnConnected;
            BlueNetManager.OnDisconnected += BlueNetManager_OnDisconnected;

            if (BlueNetManager.IsConnected())//connected event already sent so must be called manualy
            {
                BlueNetManager_OnConnected(this, System.EventArgs.Empty);
                
            }
        }

        //initalize the network object
        private void BlueNetManager_OnConnected(object sender, System.EventArgs e)
        {
            if (HostIsOwner && BlueNetManager.IsHost())
            {
                p_IsLocalyOwned = true;
            }
            else if (!HostIsOwner && !BlueNetManager.IsHost())
            {
                p_IsLocalyOwned = true;
            }

            //enable or disable local player only components
            foreach (var component in LocalPlayerOnlyBehaviours)
            {
                component.enabled = IsLocalyOwned;
            }

            //Attempt to Register this NetworkObject with the Manager to send and recive network updates
            if (!BlueNetManager.RegisterNetworkObject(this))
            {
                Debug.LogError("Failed To Register " + gameObject.name + ", Network ID: " + NetworkID + " Already Exists!!");
            }
        }
        private void BlueNetManager_OnDisconnected(object sender, System.EventArgs e)
        {
            //Object is being removed so the network manager needs to know it no longer is reciving data
            BlueNetManager.RemoveNetworkObject(this);
        }


        private void OnDestroy()
        {
            if (BlueNetManager.IsConnected()) {
                BlueNetManager_OnDisconnected(this, System.EventArgs.Empty);
            }
            BlueNetManager.OnConnected -= BlueNetManager_OnConnected;
            BlueNetManager.OnDisconnected -= BlueNetManager_OnDisconnected;

        }


        //recive an rpc from the manager and propegate the message to attached components
        public void ReciveRPC(NetworkCommand networkCommand)
        {
            //Debug.Log(gameObject.name + "Recived RPC" + networkCommand.GetString(1));

            var AllArgs = networkCommand.GetArgs();

            string[] MethodArgs = new string[AllArgs.Length - 2];

            for (int i = 0; i < MethodArgs.Length; i++)
            {
                MethodArgs[i] = AllArgs[i + 2];
            }

            //Find Attached Function and call
            SendMessage(networkCommand.GetString(1), MethodArgs,SendMessageOptions.RequireReceiver);
        }

        //send an remote procedure call
        public void SendRPC(string Name,bool ExecuteOnSelf, params string[] args)
        {
            string[] Data = new string[args.Length + 2];
            Data[0] = NetworkID.ToString();
            Data[1] = Name;
            for (int i = 2; i < Data.Length; i++)
            {
                Data[i] = args[i-2];
            }

            var command = new NetworkCommand();
            command.SetData("ObjectRPC", Data);

            //Debug.Log("Sending :" + command.ToString());

            BlueNetManager.SendRPC(command);//send to other player
            if (ExecuteOnSelf)
            {
                ReciveRPC(command);//execute on this machine
            }
            
        }

       

    }
}

