using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BoneMarker : MonoBehaviour
{
    public string markerName;
    public BoneType boneType;
    public enum BoneType
    {
        Spine,
        Rib,
        Pin
    }
}