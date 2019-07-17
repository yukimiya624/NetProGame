using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体の進行を管理するマネージャ。
/// Created by Sho Yamagami.
/// </summary>
public class GameManager : GlobalSingletonMonoBehavior<GameManager>
{
    #region Field Inspector
#pragma warning disable 649

    /// <summary>
    /// GameManagerでサイクルを管理するマネージャのリスト。
    /// </summary>
    [SerializeField]
    private List<ControllableMonoBehavior> m_Managers;

#pragma warning restore 649
    #endregion



    #region Unity Callback

    protected override void OnAwake()
    {
        base.OnAwake();
    }

    protected override void OnDestroyed()
    {
        base.OnDestroyed();
    }

    private void Start()
    {
        OnInitialize();
        OnStart();
    }

    private void Update()
    {
        OnUpdate();
    }

    private void LateUpdate()
    {
        OnLateUpdate();
    }

    private void FixedUpdate()
    {
        OnFixedUpdate();
    }

    public override void OnInitialize()
    {
        m_Managers.ForEach((m) => m.OnInitialize());
    }

    public override void OnFinalize()
    {
        m_Managers.ForEach((m) => m.OnFinalize());
    }

    public override void OnStart()
    {
        m_Managers.ForEach((m) => m.OnStart());
    }

    public override void OnUpdate()
    {
        m_Managers.ForEach((m) => m.OnUpdate());
        DOTween.ManualUpdate(Time.deltaTime, Time.unscaledDeltaTime);
    }

    public override void OnLateUpdate()
    {
        m_Managers.ForEach((m) => m.OnLateUpdate());
    }

    public override void OnFixedUpdate()
    {
        m_Managers.ForEach((m) => m.OnFixedUpdate());
    }

    #endregion
}

