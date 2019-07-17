using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : SingletonMonoBehavior<TitleManager>
{
    public enum E_STATE
    {
        SCENE_ENTERING,
        WAIT_MATCH,
        MATCH,
    }

    [SerializeField]
    private MatchRequestProcess m_MatchRequestProcess;

    private StateMachine<E_STATE> m_StateMachine;



    #region Unity Callback

    public override void OnInitialize()
    {
        base.OnInitialize();

        m_MatchRequestProcess.OnClickMatchRequest += OnClickMatchRequest;

        m_StateMachine = new StateMachine<E_STATE>();
        var sceneEntering = new State<E_STATE>(E_STATE.SCENE_ENTERING);
        m_StateMachine.AddState(sceneEntering);

        var waitMatch = new State<E_STATE>(E_STATE.WAIT_MATCH);
        m_StateMachine.AddState(waitMatch);
        waitMatch.m_OnStart += OnWaitMatchStart;
        waitMatch.m_OnUpdate += OnWaitMatchUpdate;

        var match = new State<E_STATE>(E_STATE.MATCH);
        m_StateMachine.AddState(match);
        match.m_OnStart += OnMatchStart;

        m_StateMachine.Goto(E_STATE.SCENE_ENTERING);
    }

    public override void OnFinalize()
    {
        m_StateMachine.OnFinalize();

        if (m_MatchRequestProcess != null)
        {
            m_MatchRequestProcess.OnClickMatchRequest -= OnClickMatchRequest;
        }

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



    #region Wait Match

    private void OnWaitMatchStart()
    {

    }

    private void OnWaitMatchUpdate()
    {

    }

    #endregion

    #region Match

    private void OnMatchStart()
    {
        BaseSceneManager.Instance.LoadScene(BaseSceneManager.E_SCENE.BATTLE);
    }

    #endregion



    private void OnClickMatchRequest(string address)
    {
        NetproNetworkManager.Instance.RequestMatch(address, OnSeccessMatch, OnFailedMatchRequest);
    }

    private void OnMatchWait()
    {
        m_StateMachine.Goto(E_STATE.WAIT_MATCH);
    }

    private void OnFailedMatchRequest()
    {

    }

    private void OnSeccessMatch()
    {
        m_StateMachine.Goto(E_STATE.MATCH);
    }
}
