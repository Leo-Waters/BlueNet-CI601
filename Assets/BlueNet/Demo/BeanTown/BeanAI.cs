using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BlueNet;
public class BeanAI : MonoBehaviour
{
    public BlueNetObject netObject;
    public NavMeshAgent agent;

    public Transform[] Waypoints;

    private void Update()
    {
        if (netObject.IsLocalyOwned)
        {
            if(agent.hasPath==false|| agent.remainingDistance < 2)
            {
                agent.SetDestination(Waypoints[Random.Range(0, Waypoints.Length)].position);
            }
        }
    }
}
