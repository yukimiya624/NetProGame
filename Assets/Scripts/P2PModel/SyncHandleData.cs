using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public struct SyncHandleData
{
    public int id;
    public Vector3 pos;

    public SyncHandleData(int id, Vector3 pos)
    {
        this.id = id;
        this.pos = pos;
    }
}
