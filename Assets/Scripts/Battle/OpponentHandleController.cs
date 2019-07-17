using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 相手のハンドルオブジェクト。
/// Created by Kaito Takizawa
/// </summary>
public class OpponentHandleController : ControllableMonoBehavior
{
    public override void OnInitialize()
    {
        base.OnInitialize();
        transform.position = new Vector3(0, 0, 100);
    }

    public void ApplySyncHandleData(SyncHandleData data)
    {
        if (NetproNetworkManager.Instance.IsMasterClient && data.id == 0)
        {
            transform.position = data.pos;
        }

        if (!NetproNetworkManager.Instance.IsMasterClient && data.id == 1)
        {
            transform.position = data.pos;
        }
    }
}
