using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlueNet;
namespace BlueNet.Test {
    public class TestManager : MonoBehaviour
    {
        public static TestManager instance;
        private void Awake()
        {
            instance = this;
        }
        public GameObject[] AI;
        public BlueNetObject net;
        int active;

        //sort ai array so for equal ownership of active bots
        private void OnValidate()
        {
            bool ownership = false;
            foreach (var bot in AI)
            {
                bot.GetComponent<BlueNetObject>().HostIsOwner = ownership;
                ownership = !ownership;
            }


        }
        public void RpcStart(string[] args)
        {
            active = int.Parse(args[0]);
            if (net.IsLocalyOwned)
            {
                net.SendRPC("RpcStart",false, args[0]);
            }
            for (int i = 0; i < active; i++)
            {
                AI[i].SetActive(true);
            }
        }

        public void RpcStop(string[] args)
        {
            if (net.IsLocalyOwned)
            {
                net.SendRPC("RpcStop", false);
            }
            for (int i = 0; i < active; i++)
            {
                AI[i].SetActive(false);
            }
        }
    }
}
