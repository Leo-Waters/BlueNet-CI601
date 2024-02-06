using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        private void Start()
        {
            netObject = GetComponent<BlueNetObject>();
            
        }

        //sets a bool on the animator on all players
        public void SetBool(string name,bool value)
        {
            if (netObject.IsLocalyOwned )
            {
                if(animator.GetBool(name) != value)
                {
                    netObject.SendRPC("RpcSetBool", true, name, value.ToString());
                }
                
            }
            else
            {
                Debug.LogWarning("Tried to set animator bool on object not owned localy");
            }  
        }
        //receives a bool to set on the animator
        public void RpcSetBool(string[] args)
        {
            animator.SetBool(args[0], bool.Parse(args[1]));
        }

        //sets a float on the animator on all players
        public void SetFloat(string name, float value)
        {
            if (netObject.IsLocalyOwned)
            {
                if (animator.GetFloat(name) != value) {
                    netObject.SendRPC("RpcSetFloat", true, name, value.ToString());
                }
            }
            else
            {
                Debug.LogWarning("Tried to set animator float on object not owned localy");
            }
        }
        //receives a float to set on the animator
        public void RpcSetFloat(string[] args)
        {
            animator.SetFloat(args[0], float.Parse(args[1]));
        }

        //sets a Int on the animator on all players
        public void SetInt(string name, int value)
        {
            if (netObject.IsLocalyOwned)
            {
                if (animator.GetInteger(name) != value)
                {
                    netObject.SendRPC("RpcSetInt",true, name, value.ToString());
                }
                
            }
            else
            {
                Debug.LogWarning("Tried to set animator int on object not owned localy");
            }
        }
        //receives a Int to set on the animator
        public void RpcSetInt(string[] args)
        {
            animator.SetInteger(args[0], int.Parse(args[1]));
        }

    }
}