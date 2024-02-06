using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Predicitontest : MonoBehaviour
{
    public GameObject Last, current, prediction, Smoothed;

    public float Speed = 0.1f;
    public float PacketRate = 1f;

    public float LastUpdateTime = 0;

    public Vector3 LastPerdiction;

    //simulate packet rate
    private IEnumerator Start()
    {
        Last.transform.position = current.transform.position = prediction.transform.position = Smoothed.transform.position;
        while (enabled)
        {
            yield return new WaitForSeconds(PacketRate);

            //recived a move
            Vector3 lastpos = current.transform.position;
            current.transform.position += new Vector3(0, 0, Time.deltaTime * Speed);

            //predict next move based on new position and last
            Vector3 Prediction = current.transform.position + (current.transform.position - Last.transform.position);

            //visualize perfiction
            LastPerdiction = prediction.transform.position;
            prediction.transform.position = Prediction;



            //update last pos
            Last.transform.position= lastpos;
            LastUpdateTime=Time.time;
        }
    }

    private void Update()
    {
        float t = Time.time-LastUpdateTime;

        //simulate movement
        Smoothed.transform.position = Vector3.LerpUnclamped(LastPerdiction, prediction.transform.position, t);

    }
}
