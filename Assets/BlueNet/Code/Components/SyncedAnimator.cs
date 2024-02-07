using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlueNet.DataTypes;
namespace BlueNet
{
    // synced Animator component
    // essentialy a middle man for the animator component
    // provides set value functions, alowing the animator values to be syncronized
    [RequireComponent(typeof(BlueNetObject))]
    public class SyncedAnimator : MonoBehaviour
    {
        BlueNetObject netObject;
        public Animator animator;

        

        //stores latest property changes to ensure only latest change is sent on update period
        // s1= animPropertyname s2=animvalue
        Dictionary<string, AnimProperty> UpdateBuffer = new Dictionary<string, AnimProperty>();

        [Range(0.5f, 1.5f)]
        public float VaribleSyncPeriod = 1;
        float syncDelay = 0;

        public void FixedUpdate()
        {
            if (netObject.IsLocalyOwned)
            {
                syncDelay+=Time.fixedDeltaTime;
                if (syncDelay >= VaribleSyncPeriod)
                {
                    List<string> Params = new List<string>();
                    foreach (var AnimProperty in UpdateBuffer)
                    {
                        if (AnimProperty.Value.ChangedCheck())
                        {
                            Params.Add(AnimProperty.Key);//property name
                            Params.Add(((byte)AnimProperty.Value.get_type).ToString());//property type
                            Params.Add(AnimProperty.Value.get_value);//property value
                        }
                    }


                    if (Params.Count != 0)
                    {
                        netObject.SendRPC("RpcUpdateAnimations", false, Params.ToArray());
                    }
                    
                    syncDelay = 0;
                }
            } 
        }
        //recive animation updates from objects owning player
        public void RpcUpdateAnimations(string[] args)
        {
            for (int i = 0; i < args.Length; i+=3)
            {
                var animType = (AnimPropertyType)byte.Parse(args[i + 1]);
                switch (animType)
                {
                    case AnimPropertyType.boolean:
                        animator.SetBool(args[i], bool.Parse(args[i+2]));
                        break;
                    case AnimPropertyType.interger:
                        animator.SetInteger(args[i], int.Parse(args[i + 2]));
                        break;
                    case AnimPropertyType.floating:
                        animator.SetFloat(args[i], float.Parse(args[i + 2]));
                        break;
                    default:
                        Debug.LogWarning("tried to sync non defined anim type");
                        break;
                }
            }
        }


        private void Start()
        {
            netObject = GetComponent<BlueNetObject>();
            
        }

        //sets a bool on the animator on all players
        public void SetBool(string name,bool value)
        {
            if (netObject.IsLocalyOwned )
            {
                if (UpdateBuffer.ContainsKey(name))
                {
                    UpdateBuffer[name].SetValue(value);
                    animator.SetBool(name, value);
                }
                else
                {
                    var property = new AnimProperty();
                    property.SetValue(value);
                    UpdateBuffer.Add(name, property);
                }
            }
            else
            {
                Debug.LogWarning("Tried to set animator bool on object not owned localy");
            }  
        }


        //sets a float on the animator on all players
        public void SetFloat(string name, float value)
        {
            if (netObject.IsLocalyOwned)
            {
                if (UpdateBuffer.ContainsKey(name))
                {
                    UpdateBuffer[name].SetValue(value);
                    animator.SetFloat(name, value);
                }
                else
                {
                    var property = new AnimProperty();
                    property.SetValue(value);
                    UpdateBuffer.Add(name, property);
                }
            }
            else
            {
                Debug.LogWarning("Tried to set animator float on object not owned localy");
            }
        }


        //sets a Int on the animator on all players
        public void SetInt(string name, int value)
        {
            if (netObject.IsLocalyOwned)
            {
                if (UpdateBuffer.ContainsKey(name))
                {
                    UpdateBuffer[name].SetValue(value);
                    animator.SetInteger(name, value);
                }
                else
                {
                    var property = new AnimProperty();
                    property.SetValue(value);
                    UpdateBuffer.Add(name, property);
                }
            }
            else
            {
                Debug.LogWarning("Tried to set animator int on object not owned localy");
            }
        }



    }
}