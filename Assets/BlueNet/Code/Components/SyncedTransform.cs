/*--------------------------------------------------------
 *  BlueNet A Bluetooth Multiplayers solution for unity.
 *  https://github.com/Leo-Waters/BlueNet-CI601
 * 
 *  Script: SyncedTransform
 *  Created by leo waters
 *  University Contact Email: l.waters3@uni.brighton.ac.uk
 *  Personal Contact Email li0nleo117@gmail.com
 * -------------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BlueNet{
    // synced transform component
    // provides client side perdiction while
    // automaticly sending updates of the objects transform
    [RequireComponent(typeof(BlueNetObject))]
    [AddComponentMenu("BlueNet/Synced Transform")]
    public class SyncedTransform : MonoBehaviour
    {
        BlueNetObject netObject;
        Vector3 LastPosition;
        Vector3 PredictedPosition;

        

        Vector3 RotationEuler;
        Vector3 Scale;


        //time in seconds that updates should be sent, lower the number the more acurate the position.
        [Range(0.1f,2)]
        public float UpdateRate = 1;

        //time since last position update
        float LastPositionTime;

        public float TeleportDistance = 2;

        //should the position be perdicted between updates, false = delayed movement
        [Header("Perdict the objects position between updates")]
        public bool UsePositionPerdiction=true;
        //Send velocity instead of perdict the velocity
        public bool sendVelocity = false;
        public Vector3 velocity;

        private void Start()
        {
            netObject = GetComponent<BlueNetObject>();
            PredictedPosition = LastPosition = transform.position;
            RotationEuler = transform.rotation.eulerAngles;
        }
        float UpdateDelay = 0;
        float TimeSinceLastMove;
        private void Update()
        {
            if (!netObject.IsLocalyOwned)
            {
                if (UsePositionPerdiction)
                {
                    TimeSinceLastMove = ((Time.time - LastPositionTime)/ UpdateRate);
                    //simulate movement
                    transform.position = Vector3.Lerp(LastPosition, PredictedPosition, TimeSinceLastMove);
                }

                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(RotationEuler.x, RotationEuler.y, RotationEuler.z),Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (netObject.IsLocalyOwned)
            {
                if (UpdateDelay <= 0)
                {
                    UpdateDelay = UpdateRate;
                    //has the position changed, if true send update
                    if (transform.position != LastPosition)
                    {
                        Vector3 diff;
                        if (sendVelocity)
                        {
                            diff = velocity;
                            diff *= (BlueNetManager.ping/2);
                            PredictedPosition = LastPosition = transform.position;
                            netObject.SendRPC("RpcPosition", false, Converters.Vector3ToString(LastPosition + diff),Converters.Vector3ToString(velocity));
                        }
                        else
                        {
                            //perdict where this object might be by the time its recived
                            diff = (transform.position - LastPosition);
                            diff *= (BlueNetManager.ping / 2);

                            PredictedPosition = LastPosition = transform.position;
                            netObject.SendRPC("RpcPosition", false, Converters.Vector3ToString(LastPosition + diff));
                        }
                        



                    }
                    //has the rotation changed, is true send update
                    if (transform.localEulerAngles != RotationEuler)
                    {
                        RotationEuler = transform.localEulerAngles;
                        netObject.SendRPC("RpcRotation", false, Converters.Vector3ToString(RotationEuler));
                    }
                    //has the scale changed, if true send update
                    if (transform.localScale != Scale)
                    {
                        Scale = transform.localScale;
                        netObject.SendRPC("RpcScale", false, Converters.Vector3ToString(Scale));

                    }
                }

                UpdateDelay-=Time.fixedDeltaTime;
            }
        }

        private void RpcPosition(string[] Pos_S)
        {
            Vector3 CurrentPosition = Converters.Vector3FromString(Pos_S[0]);
            if (UsePositionPerdiction)
            {
                //out of sync-- snap back to know location 
                if (Vector3.Distance(transform.position, CurrentPosition) > TeleportDistance)
                {
                    PredictedPosition = transform.position = CurrentPosition;
                }

                //Update last position
                LastPosition = transform.position;
                Vector3 diff;
                if (sendVelocity)
                {
                     diff = Converters.Vector3FromString(Pos_S[1])* UpdateDelay;
                }
                else
                {
                     diff = (CurrentPosition - LastPosition);
                }
                

                //predict next move based on new position and last
                Vector3 Prediction = CurrentPosition + diff;

                //swap perdiction data for updated lerp constraints
                PredictedPosition = Prediction;
                //used to lerp between last and new location
                LastPositionTime = Time.time;
            }
            else
            {
                PredictedPosition = CurrentPosition;
            }

        }

        private void RpcScale(string[] Scale_S)
        {
            transform.localScale= Scale = Converters.Vector3FromString(Scale_S[0]);
        }
        private void RpcRotation(string[] Rot_S)
        {
            RotationEuler = Converters.Vector3FromString(Rot_S[0]);
        }

    }
}