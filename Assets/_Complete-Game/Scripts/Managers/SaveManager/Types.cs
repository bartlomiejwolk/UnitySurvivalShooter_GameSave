using UnityEngine;
using System.Collections;

[System.Serializable]
public class SVector3
{
    public float X;
    public float Y;
    public float Z;

    public SVector3(Vector3 source)
    {
        X = source.x;
        Y = source.y;
        Z = source.z;
    }

    public SVector3(float setX, float setY, float setZ)
    {
        X = setX;
        Y = setY;
        Z = setZ;
    }

    public Vector3 Base()
    {
        return new Vector3(X, Y, Z);
    }
}
