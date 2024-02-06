using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using System;

namespace BlueNet.Demo
{
    
    public class BlueChat : MonoBehaviour
    {
        [SerializeField]
        BlueNetObject netObject;

        public GameObject Menu;
        public GameObject TextPrefab;
        public Transform Content;
        public UnityEngine.UI.ScrollRect scroll;

        public TMP_InputField TextBox;

        private void OnEnable()
        {
            BlueNetManager.OnConnected += BlueNetManager_OnConnected;
            BlueNetManager.OnDisconnected += BlueNetManager_OnDisconected;
        }

        public void LoadDemo(int Index)
        {
            BlueNetManager.ChangeScene(Index);
        }

        private void BlueNetManager_OnDisconected(object sender, EventArgs e)
        {
            Menu.SetActive(false);
        }

        private void BlueNetManager_OnConnected(object sender, System.EventArgs e)
        {
            Menu.SetActive(true);
        }

        private void OnDisable()
        {
            BlueNetManager.OnConnected -= BlueNetManager_OnConnected;
            BlueNetManager.OnDisconnected -= BlueNetManager_OnDisconected;
        }

        private void Awake()
        {
            TextPrefab.SetActive(false);
        }
        public void Exit()
        {
            BlueNetManager.CloseClient();
            Menu.SetActive(false);
        }

        public void SendChatMessageField(string value)
        {
            if (value.Contains("\n"))
            {
                SendChatMessage();
            }
        }

        public void RPC_SendMessage(object[] args)
        {
            AddMessage((string)args[0]);
        }

        public void SendChatMessage()
        {
            AddMessage(TextBox.text);
            netObject.SendRPC("RPC_SendMessage",false, TextBox.text);
            TextBox.text = "";
        }


        void AddMessage(string Value)
        {
            GameObject Message = Instantiate(TextPrefab, Content,false);
            Message.GetComponent<TextMeshProUGUI>().text = Value;
            Message.SetActive(true);
            Destroy(Message, 60);
            if (ScrollRoutine != null)
            {
                StopCoroutine(ScrollRoutine);
            }
            ScrollRoutine = StartCoroutine(ScrolltooBottom());


        }
        Coroutine ScrollRoutine;
        IEnumerator ScrolltooBottom()
        {
            yield return new WaitForEndOfFrame();
            scroll.normalizedPosition = new Vector2(0, 0);
        }
    }
}