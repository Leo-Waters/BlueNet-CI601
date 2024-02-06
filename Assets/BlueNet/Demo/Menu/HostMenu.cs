using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BlueNet.Default
{
    public class HostMenu : MonoBehaviour
    {
        private void OnEnable()
        {
            BlueNetManager.StartServer();
            
        }

        private void OnDisable()
        {
            if (BlueNetManager.IsConnected() == false&& BlueNetManager.IsHost())
            {
                BlueNetManager.CloseServer();
            }
            
        }
    }
}