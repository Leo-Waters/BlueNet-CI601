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
        Dictionary<GameObject, KeyValuePair<string,string>> DeviceList = new Dictionary<GameObject, KeyValuePair<string, string>>();
        GameObject Selected;

        public Color32 NotSelectedColor,SelectedColor;

        private void OnEnable()
        {

            ListPrefab.SetActive(false);
            SearchingDisplay.SetActive(false);
            Refresh();
        }



        void AddDevice(KeyValuePair<string, string> device)
        {
            GameObject Listing = Instantiate(ListPrefab, Content, false);

            Listing.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = device.Key;

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

            foreach (var device in BlueNetManager.GetDevices())
            {
                AddDevice(device);
            }
        }

        public void JoinSelected()
        {
            if (Selected != null)
            {
                BlueNetManager.StartClient(DeviceList[Selected].Value);
            }
            
        }
    }
}