/*--------------------------------------------------------
 *  BlueNet A Bluetooth Multiplayers solution for unity.
 *  https://github.com/Leo-Waters/BlueNet-CI601
 * 
 *  Script: Car AI for Demo
 *  Created by leo waters
 *  University Contact Email: l.waters3@uni.brighton.ac.uk
 *  Personal Contact Email li0nleo117@gmail.com
 * -------------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BlueNet;
namespace BlueNet.Demos {
    public class CarAI : MonoBehaviour
    {
        public BlueNetObject netObject;
        public NavMeshAgent agent;

        
        
        public SyncedTransform SyncedTransform;
        public Transform[] waypoints;
        public int waypointsindex = 0;
        private void Update()
        {
            if (netObject.IsLocalyOwned)
            {
                if (agent.hasPath == false || agent.remainingDistance < 2)
                {
                    agent.SetDestination(waypoints[waypointsindex].position);
                    waypointsindex++;
                    if (waypoints.Length == waypointsindex)
                    {
                        waypointsindex = 0;
                    }

                }
                SyncedTransform.velocity = agent.velocity;
            }
        }
    }
}
