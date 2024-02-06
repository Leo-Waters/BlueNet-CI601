using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlueNet;

namespace BlueNet.Demos.pong
{
    public class PongPlayer : MonoBehaviour
    {
        public BlueNetObject netObj;
        public Rigidbody2D physics;
        public float Speed = 5;

        private void Start()
        {
            if (netObj.IsLocalyOwned == false)
            {
                physics.isKinematic = true;
            }
        }
        void Update()
        {
            if (netObj.IsLocalyOwned)
            {

                if (Input.GetMouseButton(0))
                {
                    MoveTowards(Input.mousePosition);
                }
                else if (Input.touchCount > 0)
                {
                    MoveTowards(Input.touches[0].position);
                }

            }
        }

        void MoveTowards(Vector3 screenPos)
        {
            var pos= Camera.main.ScreenToWorldPoint(screenPos);

            Vector3 dir = (pos - transform.position)* Speed;
            physics.velocity = dir;
        }
    }
}