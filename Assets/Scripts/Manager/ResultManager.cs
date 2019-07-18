using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// リザルトシーンの管理クラス。
/// Created by Sho Yamagami.
/// </summary>
public class ResultManager : SingletonMonoBehavior<ResultManager>
{
    public enum E_STATE
    {
        SCENE_ENTERING,
        WAIT_MATCH,
        MATCH,
    }


    #region Field Inspector

    [SerializeField]
    private Text m_ResultText;

    [SerializeField]
    private Text m_SelfPointText;

    [SerializeField]
    private Text m_OpponentPointText;

    [SerializeField]
    private Text m_MatchWaitText;

    [SerializeField]
    private Button m_GotoTitleButton;

    [SerializeField]
    private Button m_MatchButton;

    #endregion



    #region Field

    private StateMachine<E_STATE> m_StateMachine;

    #endregion



    #region Unity Callback

    public override void OnInitialize()
    {
        base.OnInitialize();

        m_StateMachine = new StateMachine<E_STATE>();
        var sceneEntering = new State<E_STATE>(E_STATE.SCENE_ENTERING);
        m_StateMachine.AddState(sceneEntering);
        sceneEntering.m_OnStart += OnStartSceneEntering;
        sceneEntering.m_OnEnd += OnEndSceneEntering;

        var waitMatch = new State<E_STATE>(E_STATE.WAIT_MATCH);
        m_StateMachine.AddState(waitMatch);
        waitMatch.m_OnStart += OnStartWaitMatch;

        var match = new State<E_STATE>(E_STATE.MATCH);
        m_StateMachine.AddState(match);
        match.m_OnStart += OnStartMatch;

        m_StateMachine.Goto(E_STATE.SCENE_ENTERING);



        if (m_MatchWaitText)
        {
            m_MatchWaitText.gameObject.SetActive(false);
        }

        var selfPoint = GameManager.Instance.SelfGainPoint;
        var opponentPoint = GameManager.Instance.OpponentGainPoint;

        if (selfPoint > opponentPoint)
        {
            m_ResultText.text = "<color=#ff0000>YOU WIN!</color>";
        }
        else if (selfPoint < opponentPoint)
        {
            m_ResultText.text = "<color=#1010dd>YOU LOSE...</color>";
        }
        else
        {
            m_ResultText.text = "<color=#10ee50>DRAW</color>";
        }

        m_SelfPointText.text = selfPoint.ToString();
        m_OpponentPointText.text = opponentPoint.ToString();
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
    }

    #endregion



    #region Scene Entering

    private void OnStartSceneEntering()
    {
        m_GotoTitleButton.onClick.AddListener(OnClickGotoTitleButton);
        m_MatchButton.onClick.AddListener(OnClickMatchButton);
    }

    private void OnEndSceneEntering()
    {
        m_GotoTitleButton.onClick.RemoveAllListeners();
        m_MatchButton.onClick.RemoveAllListeners();
        m_GotoTitleButton.gameObject.SetActive(false);
        m_MatchButton.gameObject.SetActive(false);
    }

    #endregion



    #region Wait Match

    private void OnStartWaitMatch()
    {
        if (m_MatchWaitText)
        {
            m_MatchWaitText.gameObject.SetActive(true);
        }
    }

    #endregion



    #region Match

    private void OnStartMatch()
    {
        BaseSceneManager.Instance.LoadScene(BaseSceneManager.E_SCENE.BATTLE);
    }

    #endregion/// <summary>




    private void OnClickGotoTitleButton()
    {
        BaseSceneManager.Instance.LoadScene(BaseSceneManager.E_SCENE.TITLE);
    }


    /// <summary>
    /// マッチリクエストボタンを押した時の処理
    /// </summary>
    private void OnClickMatchButton()
    {
        var address = NetproNetworkManager.Instance.SelfIpAddress.ToString();
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
