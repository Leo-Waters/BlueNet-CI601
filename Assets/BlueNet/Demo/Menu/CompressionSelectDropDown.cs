/*--------------------------------------------------------
 *  BlueNet A Bluetooth Multiplayers solution for unity.
 *  https://github.com/Leo-Waters/BlueNet-CI601
 * 
 *  Script: Drop Down for selecting compression algorithm
 *  Created by leo waters
 *  University Contact Email: l.waters3@uni.brighton.ac.uk
 *  Personal Contact Email li0nleo117@gmail.com
 * -------------------------------------------------------
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlueNet;
namespace BlueNet.Default
{
    public class CompressionSelectDropDown : MonoBehaviour
    {
        public void ValueChanged(int val)
        {
            BlueNetManager.Instance.SetCompressionAlgoithm((Compression.CompressionAlgorithmType)val);
        }
    }
}