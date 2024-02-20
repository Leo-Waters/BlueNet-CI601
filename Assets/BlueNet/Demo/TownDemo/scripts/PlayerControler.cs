using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlueNet;
namespace BlueNet.Demos
{
    public class PlayerControler : MonoBehaviour
    {
        public CharacterController controller;
        public SyncedTransform syncedTransform;
        public Transform cameraTrans;

        public float walkSpeed = 1;
        public float RunSpeed = 2;
        bool Running = false;


        private float MoveSpeed {
            get {
                if (Running)
                {
                    return RunSpeed;
                }
                else
                {
                    return walkSpeed;
                }
            } 
        }
        
        public float LookSpeed = 2;
        public Vector2 UpDownClamp;
        float LookUpRot = 0;

 
        public SyncedAnimator animator;

        private void OnEnable()
        {
            controller.enabled = true;
        }
        private void OnDisable()
        {
            controller.enabled = false;
        }

        void Update()
        {
            //is the player holding shift, then they are running
            Running = TownCharacterControls.Instance.Running;

            //rotate
            Vector2 rot = TownCharacterControls.Instance.LookDir * LookSpeed;

            //RotY
            LookUpRot = Mathf.Clamp((LookUpRot - rot.y), UpDownClamp.x, UpDownClamp.y);

            cameraTrans.localRotation = Quaternion.Euler(LookUpRot, 0, 0);

            //RotX
            transform.Rotate(0, rot.x, 0);

            //get input direction
            Vector2 Dir = TownCharacterControls.Instance.moveDir;
            Dir.Normalize();//ensure fixed speed in every direction

            animator.SetFloat("y", Running ? Dir.y : (Dir.y*2) );
            animator.SetFloat("x", Dir.x);

            //move
            Vector3 move = new Vector3();
            move += transform.right * Dir.x * MoveSpeed;
            move += transform.forward * Dir.y * MoveSpeed;

            controller.SimpleMove(move);
            syncedTransform.velocity=controller.velocity;
        }

    }
}