using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlueNet;
using BlueNet.DataTypes;

namespace BlueNet.Demos.pong {
    public class PongBall : MonoBehaviour
    {
        public BlueNetObject netObj;
        public Rigidbody2D physics;
        public SyncedTransform syncedTransform;
        public float speed=1;
        int SideHits;
        int player1Score = 0, player2Score = 0;
        public TMPro.TextMeshProUGUI player1Score_Text, player2Score_Text,Winner_Text;
        public GameObject GameOveMenu,readyButton;
        void Display()
        {
            player1Score_Text.text = "Score: " + player1Score.ToString();
            player2Score_Text.text = "Score: " + player2Score.ToString();
        }

        void Start()
        {
            Ready1 = Ready2 = false;
            GameOveMenu.SetActive(true);
            readyButton.SetActive(true);
            Winner_Text.text = "";
            netObj.OnSyncVarUpdate += NetObj_SyncVarUpdate;
        }

        public void Exit()
        {
            BlueNetManager.CloseClient();
        }

        bool Ready1, Ready2;
        public void Readyup()
        {
            readyButton.SetActive(false);
            if (netObj.IsLocalyOwned)
            {
                Ready1 = true;
            }
            else
            {
                netObj.SendRPC("RPC_ReadyUp", false);
            }
        }
        public void RPC_ReadyUp()
        {
            Ready2 = true;
        }

        public LayerMask mask;
        private void FixedUpdate()
        {
            if (Ready1 && Ready2)
            {
                StartGame();
            }

            if (netObj.IsLocalyOwned)
            {
                if (Physics2D.Raycast(transform.position, physics.velocity, physics.velocity.magnitude, mask))
                {
                    //going to hit object so perdict needs inverse velocity
                    syncedTransform.velocity = -(physics.velocity);
                }
                else
                {
                    syncedTransform.velocity = physics.velocity;
                }
                
                if ((player1Score >= 5 || player2Score >= 5))
                {
                    PauseGame(player2Score >= 5);
                }
               
            }

        }

        void StartGame()
        {
            if (netObj.IsLocalyOwned)
            {
                Ready1 = Ready2 = false;
                GameOveMenu.SetActive(false);
                readyButton.SetActive(false);
                netObj.SendRPC("RPC_GameStart", false);
                Setup();
            }
        }
        public void RPC_GameStart()
        {
            GameOveMenu.SetActive(false);
            readyButton.SetActive(false);
            physics.isKinematic = true;
        }
        void PauseGame(bool HostWinner)
        {
            player1Score = player2Score = 0;
            physics.position = Vector2.zero;
            physics.velocity = new Vector3(0,0,0);
            Winner_Text.text = HostWinner ? "you won" : "you lost";
            GameOveMenu.SetActive(true);
            readyButton.SetActive(true);
            netObj.SendRPC("RPC_GameOver", false, HostWinner.ToString());
        }
        public void RPC_GameOver(string[] args)
        {
            bool hostIsWinner = bool.Parse(args[0]);
            GameOveMenu.SetActive(true);
            readyButton.SetActive(true);
            Winner_Text.text = !hostIsWinner ? "you won" : "you lost";
        }
        private void NetObj_SyncVarUpdate(object sender, SyncVar var)
        {
            switch (var.getTag)
            {
                case "player1Score":
                    player1Score = var.GetInt();
                    Display();
                    break;
                case "player2Score":
                    player2Score = var.GetInt();
                    Display();
                    break;
                default:
                    break;
            }


        }
        void ResetPos()
        {
            int Dir = Random.Range(0, 2) == 0 ? -1 : 1;
            physics.position = Vector2.zero;
            physics.velocity = new Vector3(speed * Dir, speed * Dir);
            SideHits = 0;
        }
        void Setup()
        {
            ResetPos();
            
            netObj.SetVar("player1Score", 0);
            netObj.SetVar("player2Score", 0);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (netObj.IsLocalyOwned) {
                switch (collision.gameObject.name)
                {
                    case "Wall_Left":
                        SideHits = 0;
                        netObj.SetVar("player1Score", player1Score+1);
                        Display();
                        break;
                    case "Wall_Right":
                        SideHits = 0;
                        netObj.SetVar("player2Score", player2Score + 1);
                        Display();
                        break;
                    case "Side":
                        SideHits++;
                        if (SideHits == 5)
                        {
                            ResetPos();
                        }
                        break;
                    default:
                        SideHits = 0;
                        break;

                }
            }
        }
    }
}