using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public struct SyncPlateData
{
    public int id;
    public Vector3 pos;
    public Vector3 vel;

    public SyncPlateData(int id, Vector3 pos, Vector3 vel)
    {
        this.id = id;
        this.pos = pos;
        this.vel = vel;
    }
   
}
