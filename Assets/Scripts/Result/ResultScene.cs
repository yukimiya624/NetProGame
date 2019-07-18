using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultScene : BaseScene
{
    /// <summary>
    /// シーンに入る直前処理
    /// </summary>
    public override void OnBeforeShow(Action onComplete)
    {
        GetManagerList().ForEach(m => m.OnInitialize());
        base.OnBeforeShow(onComplete);
    }

    /// <summary>
    /// シーンから出ていく直後処理
    /// </summary>
    public override void OnAfterHide(Action onComplete)
    {
        GetManagerList().ForEach(m => m.OnFinalize());
        NetproNetworkManager.Instance.CloseClients();
        base.OnAfterHide(onComplete);
    }
}
