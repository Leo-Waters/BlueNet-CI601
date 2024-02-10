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

        public Transform[] Waypoints;
        public SyncedAnimator animator;
        public SyncedTransform SyncedTransform;
        private void Update()
        {
            if (netObject.IsLocalyOwned)
            {
                if (agent.hasPath == false || agent.remainingDistance < 2)
                {
                    agent.SetDestination(Waypoints[Random.Range(0, Waypoints.Length)].position);

                }
                animator.SetFloat("y", agent.velocity.z);
                animator.SetFloat("x", agent.velocity.x);
                SyncedTransform.velocity = agent.velocity;
            }
        }
    }
}
