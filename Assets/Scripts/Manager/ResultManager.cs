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
        SCENE_ENTERING
    }


    #region Field Inspector

    [SerializeField]
    private Text m_ResultText;

    [SerializeField]
    private Text m_SelfPointText;

    [SerializeField]
    private Text m_OpponentPointText;

    [SerializeField]
    private Button m_GotoTitleButton;

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

        m_StateMachine.Goto(E_STATE.SCENE_ENTERING);

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
    }

    private void OnEndSceneEntering()
    {
        m_GotoTitleButton.onClick.RemoveAllListeners();
        m_GotoTitleButton.gameObject.SetActive(false);
    }

    private void OnClickGotoTitleButton()
    {
        BaseSceneManager.Instance.LoadScene(BaseSceneManager.E_SCENE.TITLE);
    }

    #endregion
}
