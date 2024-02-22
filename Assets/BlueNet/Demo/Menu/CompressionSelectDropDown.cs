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