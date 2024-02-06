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
        public float speed=1;
        int SideHits;
        int player1Score = 0, player2Score = 0;
        public TMPro.TextMeshProUGUI player1Score_Text, player2Score_Text;
        void Display()
        {
            player1Score_Text.text = "Score: " + player1Score.ToString();
            player2Score_Text.text = "Score: " + player2Score.ToString();
        }

        // Start is called before the first frame update
        void Start()
        {
            if (netObj.IsLocalyOwned)
            {
                Setup();
            }
            else
            {
                physics.isKinematic = true;
            }

            netObj.OnSyncVarUpdate += NetObj_SyncVarUpdate;
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

        void Setup()
        {
            int Dir = Random.Range(0, 2) == 0 ? -1 : 1;
            physics.position = Vector2.zero;
            physics.velocity = new Vector3(speed * Dir, speed * Dir);
            SideHits = 0;

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
                            Setup();
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