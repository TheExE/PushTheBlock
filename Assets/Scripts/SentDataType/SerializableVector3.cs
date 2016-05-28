using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class SerializableVector3
{
    private float x;
    private float y;
    private float z;

    public SerializableVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public SerializableVector3()
    {
        x = 0;
        y = 0;
        z = 0;
    }

    public SerializableVector3(Vector3 vec)
    {
        x = vec.x;
        y = vec.y;
        z = vec.z;
    }

    public SerializableVector3(Vector2 vec)
    {
        x = vec.x;
        y = vec.y;
        z = 0;
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

   public Vector3 Vect3
   {
        get { return new Vector3(x, y, z); }
        set { x = value.x; y = value.y; z = value.z; }
   }


}
