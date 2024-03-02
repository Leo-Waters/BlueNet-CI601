/*--------------------------------------------------------
 *  BlueNet A Bluetooth Multiplayers solution for unity.
 *  https://github.com/Leo-Waters/BlueNet-CI601
 * 
 *  Script: Pong Player for Demo
 *  Created by leo waters
 *  University Contact Email: l.waters3@uni.brighton.ac.uk
 *  Personal Contact Email li0nleo117@gmail.com
 * -------------------------------------------------------
 */
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
        public SyncedTransform syncedTransform;
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
                syncedTransform.velocity = physics.velocity;
            }
        }

        void MoveTowards(Vector3 screenPos)
        {
            var pos= Camera.main.ScreenToWorldPoint(screenPos);

            Vector3 dir = (pos - transform.position)* Speed;
            dir.x = 0;
            physics.velocity = dir;
        }
    }
}