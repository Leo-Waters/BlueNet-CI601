using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*--------------------------------------------------------
 *  BlueNet A Bluetooth Multiplayers solution for unity.
 *  https://github.com/Leo-Waters/BlueNet-CI601
 * 
 *  Script: SyncVar for BlueNetObject
 *  Created by leo waters
 *  University Contact Email: l.waters3@uni.brighton.ac.uk
 *  Personal Contact Email li0nleo117@gmail.com
 * -------------------------------------------------------
 */
namespace BlueNet.DataTypes
{
    [System.Serializable]
    public class SyncVar
    {
        [SerializeField]
        string tag = "";
        [SerializeField]
        string value = "";

        public bool CompareTag(string _tag)
        {
            return _tag == tag;
        }
        public string getTag { get { return tag; } }

        //should sendUpdate
        bool changed = false;

        public bool ShouldSendUpdate()
        {
            return changed;
        }
        public void SentUpdate()
        {
            changed = false;
        }

        public void NetworkUpdate(string Newvalue)
        {
            value = Newvalue;
        }

        public void Update(string Newvalue)
        {
            changed = true;
            value = Newvalue;
        }
        public void Update(int Newvalue)
        {
            changed = true;
            value = Newvalue.ToString();
        }

        public void Update(float Newvalue)
        {
            changed = true;
            value = Newvalue.ToString();
        }
        public void Update(bool Newvalue)
        {
            changed = true;
            value = Newvalue.ToString();
        }

        //getters 

        public string GetString()
        {
            return value;
        }
        public int GetInt()
        {
            return int.Parse(value);
        }
        public float GetFloat()
        {
            return float.Parse(value);
        }
        public bool GetBool()
        {
            return bool.Parse(value);
        }
    }
}