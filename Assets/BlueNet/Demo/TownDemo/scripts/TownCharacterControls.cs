/*--------------------------------------------------------
 *  BlueNet A Bluetooth Multiplayers solution for unity.
 *  https://github.com/Leo-Waters/BlueNet-CI601
 * 
 *  Script: Town Character Controls for demo
 *  Created by leo waters
 *  University Contact Email: l.waters3@uni.brighton.ac.uk
 *  Personal Contact Email li0nleo117@gmail.com
 * -------------------------------------------------------
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlueNet.Demos
{

    public class TownCharacterControls : MonoBehaviour
    {
        public static TownCharacterControls Instance;
        public GameObject standardUI, TouchUI;
        public Vector2 moveDir;
        public Vector2 LookDir;
        public bool Running = false;

        private void Awake()
        {
            Instance = this;
        }
        private void Start()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            standardUI.SetActive(false);
            TouchUI.SetActive(true);
#else
            standardUI.SetActive(true);
            TouchUI.SetActive(false);
#endif
        }
#if !UNITY_ANDROID || UNITY_EDITOR
        private void FixedUpdate()
        {
            Running=Input.GetKey(KeyCode.LeftShift);
            LookDir.x = Input.GetAxis("Mouse X");
            LookDir.y = Input.GetAxis("Mouse Y");

            moveDir.x = Input.GetAxis("Horizontal");
            moveDir.y = Input.GetAxis("Vertical");

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                exit();

            }
        }
#endif

        public void Sprint()
        {
            Running =!Running;
        }
        public void exit()
        {
            BlueNetManager.CloseClient();
        }
        public void Straight(int dir)
        {
            moveDir.y = dir;
        }
        public void Horizontal(int dir)
        {
            moveDir.x = dir;
        }

        public void Turn(int dir)
        {
            LookDir.x = dir;
        }
        public void look(int dir)
        {
            LookDir.y = dir;
        }
    }
}