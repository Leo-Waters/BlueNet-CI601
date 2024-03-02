/*--------------------------------------------------------
 *  BlueNet A Bluetooth Multiplayers solution for unity.
 *  https://github.com/Leo-Waters/BlueNet-CI601
 * 
 *  Script: Host Menu For Demo
 *  Created by leo waters
 *  University Contact Email: l.waters3@uni.brighton.ac.uk
 *  Personal Contact Email li0nleo117@gmail.com
 * -------------------------------------------------------
 */
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