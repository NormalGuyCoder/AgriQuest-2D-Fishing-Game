using UnityEngine;

[System.Serializable]
public class BoneData
{
    public string name;
    public float x;
    public float y;
    public float width;
    public float height;
    public BoneType boneType;
    public bool isRemoved;

    public BoneData(string name, float x, float y, float width, float height, BoneType boneType)
    {
        this.name = name;
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.boneType = boneType;
        this.isRemoved = false;
    }
}

public enum BoneType
{
    Spine,
    Rib,
    Pin
}



