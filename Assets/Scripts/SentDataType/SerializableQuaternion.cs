using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class SerializableQuaternion
{
    private float x;
    private float y;
    private float z;
    private float w;

    public SerializableQuaternion()
    {
        x = 0;
        y = 0;
        z = 0;
        w = 0;
    }

    public SerializableQuaternion(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public SerializableQuaternion(Quaternion q)
    {
        x = q.x;
        y = q.y;
        z = q.z;
        w = q.w;
    }

    public Quaternion Quaternion
    {
        get { return new Quaternion(x, y, z, w); }
        set
        {
            x = value.x;
            y = value.y;
            z = value.z;
            w = value.w;
        }

    }
    public float X
    {
        get { return x; }
    }
    public float Y
    {
        get { return y; }
    }
    public float Z
    {
        get { return z; }
    }
    public float W
    {
        get { return W; }
    }
}
