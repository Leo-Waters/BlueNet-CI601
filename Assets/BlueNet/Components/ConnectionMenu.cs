using InTheHand.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BlueNet.Default
{
    //Simple Menu Script for listing devices to join
    public class ConnectionMenu : MonoBehaviour
    {
        public GameObject ListPrefab;
        public Transform Content;
        public GameObject SearchingDisplay;
        Dictionary<GameObject, BluetoothDeviceInfo> DeviceList = new Dictionary<GameObject, BluetoothDeviceInfo>();
        GameObject Selected;

        public Color32 NotSelectedColor,SelectedColor;

        private void OnEnable()
        {
            BlueNetManager.RecivedNewDeviceList += BlueNetManager_RecivedDeviceList;
            ListPrefab.SetActive(false);
            SearchingDisplay.SetActive(false);
            Refresh();
        }

        private void BlueNetManager_RecivedDeviceList(object sender, IReadOnlyCollection<BluetoothDeviceInfo> Devices)
        {
            foreach (var device in Devices)
            {
                AddDevice(device);
            }
            SearchingDisplay.SetActive(false);
        }

        private void OnDisable()
        {
            BlueNetManager.RecivedNewDeviceList -= BlueNetManager_RecivedDeviceList;
        }


        void AddDevice(BluetoothDeviceInfo device)
        {
            GameObject Listing = Instantiate(ListPrefab, Content, false);

            Listing.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = device.DeviceName;

            Listing.GetComponent<UnityEngine.UI.Image>().color = NotSelectedColor;

            Listing.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                if (Selected != null)
                {
                    Selected.GetComponent<UnityEngine.UI.Image>().color = NotSelectedColor;
                }
                Selected = Listing;

                Selected.GetComponent<UnityEngine.UI.Image>().color = SelectedColor;

            });


            DeviceList.Add(Listing, device);
            Listing.transform.SetAsFirstSibling();
            Listing.SetActive(true);
        }

        public void Refresh()
        {
            foreach (var item in DeviceList)
            {
                Destroy(item.Key);
            }
            DeviceList.Clear();

            SearchingDisplay.SetActive(true);

            var Devices=BlueNetManager.GetDevices(true);

            foreach (var device in Devices)
            {
                AddDevice(device);
            }
        }

        public void JoinSelected()
        {
            if (Selected != null)
            {
                BlueNetManager.StartClient(DeviceList[Selected]);
            }
            
        }
    }
}
