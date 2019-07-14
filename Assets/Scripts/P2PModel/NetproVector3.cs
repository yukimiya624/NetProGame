using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NetproVector3
{
    public float x, y, z;

    public Vector3 GetVector3()
    {
        return new Vector3(x, y, z);
    }

    public void SetVector3(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }
}
