using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleManager : SingletonMonoBehavior<TitleManager>
{
    /// <summary>
    /// タイトルのステート
    /// </summary>
    public enum E_STATE
    {
        /// <summary>
        /// シーン遷移直後
        /// </summary>
        SCENE_ENTERING,

        /// <summary>
        /// マッチ待機
        /// </summary>
        WAIT_MATCH,

        /// <summary>
        /// マッチした
        /// </summary>
        MATCH,
    }



    #region Field Inspector
#pragma warning disable 649

    /// <summary>
    /// マッチリクエストをするためのUI
    /// </summary>
    [SerializeField]
    private MatchRequestProcess m_MatchRequestProcess;

    /// <summary>
    /// マッチ待機テキスト
    /// </summary>
    [SerializeField]
    private Text m_MatchWaitText;

    /// <summary>
    /// ゲーム終了ボタン
    /// </summary>
    [SerializeField]
    private Button m_GameEndButton;

#pragma warning restore 649
    #endregion


    /// <summary>
    /// タイトルシーンのステートマシン
    /// </summary>
    private StateMachine<E_STATE> m_StateMachine;



    #region Unity Callback

    public override void OnInitialize()
    {
        base.OnInitialize();

        m_StateMachine = new StateMachine<E_STATE>();
        var sceneEntering = new State<E_STATE>(E_STATE.SCENE_ENTERING);
        m_StateMachine.AddState(sceneEntering);
        sceneEntering.m_OnStart += OnStartSceneEntering;

        var waitMatch = new State<E_STATE>(E_STATE.WAIT_MATCH);
        m_StateMachine.AddState(waitMatch);
        waitMatch.m_OnStart += OnStartWaitMatch;

        var match = new State<E_STATE>(E_STATE.MATCH);
        m_StateMachine.AddState(match);
        match.m_OnStart += OnStartMatch;

        m_StateMachine.Goto(E_STATE.SCENE_ENTERING);

        m_GameEndButton.onClick.AddListener(Application.Quit);
    }

    public override void OnFinalize()
    {
        m_StateMachine.OnFinalize();

        base.OnFinalize();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        m_StateMachine.OnUpdate();
    }

    public override void OnLateUpdate()
    {
        base.OnLateUpdate();
        m_StateMachine.OnLateUpdate();
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        m_StateMachine.OnFixedUpdate();
    }

    #endregion



    #region Scene Entering

    private void OnStartSceneEntering()
    {
        if (m_MatchWaitText)
        {
            m_MatchWaitText.gameObject.SetActive(false);
        }

        m_MatchRequestProcess.OnClickMatchRequest += OnClickMatchRequest;
    }

    #endregion



    #region Wait Match

    private void OnStartWaitMatch()
    {
        if (m_MatchWaitText)
        {
            m_MatchWaitText.gameObject.SetActive(true);
        }

        m_MatchRequestProcess.OnClickMatchRequest -= OnClickMatchRequest;
    }

    #endregion



    #region Match

    private void OnStartMatch()
    {
        BaseSceneManager.Instance.LoadScene(BaseSceneManager.E_SCENE.BATTLE);
    }

    #endregion



    /// <summary>
    /// マッチリクエストボタンを押した時の処理
    /// </summary>
    private void OnClickMatchRequest(string address)
    {
        NetproNetworkManager.Instance.RequestMatch(address, OnSeccessMatch, OnMatchWait, OnFailedMatchRequest);
    }



    #region Async Process Callback

    /// <summary>
    /// マッチ待機開始コールバック
    /// </summary>
    private void OnMatchWait()
    {
        var state = m_StateMachine.GetCurrentState();
        if (state != null && state.m_Key == E_STATE.SCENE_ENTERING)
        {
            m_StateMachine.Goto(E_STATE.WAIT_MATCH);
        }
    }

    /// <summary>
    /// マッチリクエスト失敗コールバック
    /// </summary>
    private void OnFailedMatchRequest()
    {

    }

    /// <summary>
    /// マッチ完了コールバック
    /// </summary>
    private void OnSeccessMatch()
    {
        m_StateMachine.Goto(E_STATE.MATCH);
    }

    #endregion
}
