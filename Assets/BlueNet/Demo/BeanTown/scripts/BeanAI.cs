using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BlueNet;
namespace BlueNet.Demos {
    public class BeanAI : MonoBehaviour
    {
        public BlueNetObject netObject;
        public NavMeshAgent agent;

        public bool Route = false;
        int index = 0;
        public Transform[] Waypoints;
        public SyncedAnimator animator;
        public SyncedTransform SyncedTransform;
        private void Update()
        {
            if (netObject.IsLocalyOwned)
            {
                if (agent.hasPath == false || agent.remainingDistance < 2)
                {
                    if (Route)
                    {
                        agent.SetDestination(Waypoints[index].position);
                        index++;
                        if (index == Waypoints.Length)
                        {
                            index = 0;
                        }
                    }
                    else
                    {
                        agent.SetDestination(Waypoints[Random.Range(0, Waypoints.Length)].position);
                    }
                    

                }
                animator.SetFloat("y", 1);
                animator.SetFloat("x",0);
                SyncedTransform.velocity = agent.velocity;
            }
        }
    }
}
