using System;

public class TitleScene : BaseScene
{
    /// <summary>
    /// シーン遷移直前処理
    /// </summary>
    public override void OnBeforeShow(Action onComplete)
    {
        GetManagerList().ForEach(m => m.OnInitialize());
        base.OnBeforeShow(onComplete);
    }

    /// <summary>
    /// シーン遷移直後処理
    /// </summary>
    public override void OnAfterHide(Action onComplete)
    {
        GetManagerList().ForEach(m => m.OnFinalize());
        base.OnAfterHide(onComplete);
    }
}
