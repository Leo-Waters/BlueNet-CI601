using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BlueNet{
    // synced transform component
    // provides client side perdiction while
    // automaticly sending updates of the objects transform
    [RequireComponent(typeof(BlueNetObject))]
    public class SyncedTransform : MonoBehaviour
    {
        BlueNetObject netObject;
        public Vector3 LastPosition;
        public Vector3 PredictedPosition;

        Vector3 RotationEuler;
        Vector3 Scale;

        //time in seconds that updates should be sent, lower the number the more acurate the position.
        [Range(0.1f,2)]
        public float UpdateRate = 1;

        //time since last position update
        float LastPositionTime;

        public float TeleportDistance = 2;

        //should the position be perdicted between updates, false = snappy movement
        public bool UsePositionPerdiction=true;

        private void Start()
        {
            netObject = GetComponent<BlueNetObject>();
            PredictedPosition = LastPosition = transform.position;
            RotationEuler = transform.rotation.eulerAngles;
        }
        float UpdateDelay = 0;

        private void Update()
        {
            if (!netObject.IsLocalyOwned)
            {
                if (UsePositionPerdiction)
                {
                    float TimeSinceLastMove = Time.time - LastPositionTime;
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
                        PredictedPosition= LastPosition = transform.position;
                        netObject.SendRPC("RpcPosition", false, Converters.Vector3ToString(LastPosition));

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
                //Update last position
                LastPosition = transform.transform.position;

                Vector3 diff = (CurrentPosition - LastPosition);

                //TODO -- compensate current position network delay add diff * response delay

                //predict next move based on new position and last
                Vector3 Prediction = CurrentPosition + diff;

                //swap perdiction data for updated lerp constraints
                PredictedPosition = Prediction;
                //used to lerp between last and new location
                LastPositionTime = Time.time;

                //out of sync-- snap back to know location 
                if (Vector3.Distance(transform.position, CurrentPosition) > TeleportDistance)
                {
                    //LastPerdiction= PredictedPosition=transform.position = CurrentPosition;
                }
            }
            else
            {
                transform.position = CurrentPosition;
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