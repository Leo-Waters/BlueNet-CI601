using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BlueNet.DataTypes
{
    //type of animation property
    public enum AnimPropertyType : byte
    {
        boolean = 0,
        floating = 1,
        interger = 2
    }


    //anim property class
    //stores information about a syncronized animation property
    //such as type and value, also if the value has changed
    //used by synced Animator component to identify which values to sync
    public class AnimProperty
    {

        bool Changed = false;
        //check if should update and reset changed bool to false if it should update
        public bool ChangedCheck()
        {
            if (Changed)
            {
                Changed = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        string value;
        public string get_value
        {
            get
            {
                return value;
            }
        }

        AnimPropertyType type;
        //what type of animation property is this
        public AnimPropertyType get_type
        {
            get
            {
                return type;
            }
        }
        public void SetValue(bool b_value)
        {
            string new_value = b_value.ToString();
            if (value != new_value)
            {
                value = new_value;
                type = AnimPropertyType.boolean;
                Changed = true;
            }
        }
        public void SetValue(float f_value)
        {
            string new_value = f_value.ToString();
            if (value != new_value)
            {
                value = new_value;
                type = AnimPropertyType.floating;
                Changed = true;
            }
        }
        public void SetValue(int i_value)
        {
            string new_value = i_value.ToString();
            if (value != new_value)
            {
                value = new_value;
                type = AnimPropertyType.interger;
                Changed = true;
            }
        }
    }
}