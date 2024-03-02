/*--------------------------------------------------------
 *  BlueNet A Bluetooth Multiplayers solution for unity.
 *  https://github.com/Leo-Waters/BlueNet-CI601
 * 
 *  Script: Converters for commonly used data types
 *  Created by leo waters
 *  University Contact Email: l.waters3@uni.brighton.ac.uk
 *  Personal Contact Email li0nleo117@gmail.com
 * -------------------------------------------------------
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlueNet
{
    public static class Converters
    {
        //Convert string from network into a usable vector
        public static Vector3 Vector3FromString(string s)
        {
            //split x y z
            var vars = s.Split("*");
            //create and return new vector from string data
            return new Vector3(float.Parse(vars[0]), float.Parse(vars[1]), float.Parse(vars[2]));
        }
        //Convert string from network into a usable vector
        public static Vector2 Vector2FromString(string s)
        {
            //split x y z
            var vars = s.Split("*");
            //create and return new vector from string data
            return new Vector2(float.Parse(vars[0]), float.Parse(vars[1]));
        }
        //convert vector into a string for network transport
        public static string Vector3ToString(Vector3 vec3)
        {
            return vec3.x + "*" + vec3.y + "*"+vec3.z;
        }
        //convert vector into a string for network transport
        public static string Vector2ToString(Vector2 vec2)
        {
            return vec2.x + "*" + vec2.y;
        }
    }
}