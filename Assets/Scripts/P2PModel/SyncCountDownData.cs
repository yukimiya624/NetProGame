using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct SyncCountDownData
{
    public int CountDown;
    public DateTime SendTime;

    public SyncCountDownData(int CountDown, DateTime SendTime)
    {
        this.CountDown = CountDown;
        this.SendTime = SendTime;
    }
}
