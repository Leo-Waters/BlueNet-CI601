/*--------------------------------------------------------
 *  BlueNet A Bluetooth Multiplayers solution for unity.
 *  https://github.com/Leo-Waters/BlueNet-CI601
 * 
 *  Script: Connection Menu for Demo
 *  Created by leo waters
 *  University Contact Email: l.waters3@uni.brighton.ac.uk
 *  Personal Contact Email li0nleo117@gmail.com
 * -------------------------------------------------------
 */
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
        Dictionary<GameObject, KeyValuePair<string,string>> DeviceList = new Dictionary<GameObject, KeyValuePair<string, string>>();
        GameObject Selected;

        public Color32 NotSelectedColor,SelectedColor;

        private void OnEnable()
        {
            ListPrefab.SetActive(false);
            Refresh();
        }

        void AddDevice(KeyValuePair<string, string> device)
        {
            GameObject Listing = Instantiate(ListPrefab, Content, false);

            //set text to device name
            Listing.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = device.Key;
            //set color to not selected color
            Listing.GetComponent<UnityEngine.UI.Image>().color = NotSelectedColor;
            //add click event
            Listing.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                if (Selected != null)//de select preivious device
                {
                    Selected.GetComponent<UnityEngine.UI.Image>().color = NotSelectedColor;
                }
                Selected = Listing;
                //set color as selected
                Selected.GetComponent<UnityEngine.UI.Image>().color = SelectedColor;

            });


            DeviceList.Add(Listing, device);
            Listing.transform.SetAsFirstSibling();
            Listing.SetActive(true);
        }

        //get paired devices and list
        public void Refresh()
        {
            foreach (var item in DeviceList)
            {
                Destroy(item.Key);
            }
            DeviceList.Clear();

            foreach (var device in BlueNetManager.GetDevices())
            {
                AddDevice(device);
            }
        }
        //join selected device
        public void JoinSelected()
        {
            if (Selected != null)
            {
                BlueNetManager.StartClient(DeviceList[Selected].Value);
            }
            
        }
    }
}
