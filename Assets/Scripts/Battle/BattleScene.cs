using System;

public class BattleScene : BaseScene
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