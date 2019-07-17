using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Battleシーンを管理するマネージャ。
/// </summary>
public class BattleManager : SingletonMonoBehavior<BattleManager>
{
    /// <summary>
    /// バトルのステート
    /// </summary>
    public enum E_STATE
    {
        /// <summary>
        /// シーン遷移直後
        /// </summary>
        SCENE_ENTERING,

        /// <summary>
        /// カウントダウン中
        /// </summary>
        COUNT_DOWN,

        /// <summary>
        /// バトル開始
        /// </summary>
        BATTLE_START,

        /// <summary>
        /// バトル中
        /// </summary>
        BATTLE,

        /// <summary>
        /// バトル終了
        /// </summary>
        BATTLE_END,

        /// <summary>
        /// 通信エラー発生
        /// </summary>
        CONNECT_ERROR,
    }



    #region Field Inspector
#pragma warning disable 649

    /// <summary>
    /// 自身のハンドルオブジェクトのプレハブ
    /// </summary>
    [SerializeField]
    private SelfHandleController m_SelfHandlePrefab;

    /// <summary>
    /// 相手のハンドルオブジェクトのプレハブ
    /// </summary>
    [SerializeField]
    private OpponentHandleController m_OpponentHandlePrefab;

    /// <summary>
    /// プレートオブジェクトのプレハブ
    /// </summary>
    [SerializeField]
    private Plate m_PlatePrefab;

    [SerializeField]
    private Text m_Text;

    /// <summary>
    /// プレートの最初の位置
    /// </summary>
    [SerializeField]
    private Vector3 m_StartPlatePosition;

    /// <summary>
    /// カウントダウン時間
    /// </summary>
    [SerializeField]
    private int m_CountDownTime;

    /// <summary>
    /// バトルの時間
    /// </summary>
    [SerializeField]
    private int m_BattleTime;

#pragma warning restore 649
    #endregion



    #region Field

    /// <summary>
    /// タイトルシーンのステートマシン
    /// </summary>
    private StateMachine<E_STATE> m_StateMachine;

    /// <summary>
    /// 自身のハンドルオブジェクト
    /// </summary>
    private SelfHandleController m_SelfHandle;

    /// <summary>
    /// 相手のハンドルオブジェクト
    /// </summary>
    private OpponentHandleController m_OpponentHandle;

    /// <summary>
    /// プレートオブジェクト
    /// </summary>
    private Plate m_Plate;

    /// <summary>
    /// カウントダウンタイマー
    /// </summary>
    private Timer m_CountDownTimer;

    /// <summary>
    /// バトル中のタイマー
    /// </summary>
    private Timer m_BattleTimer;

    private int m_CountDown;

    private int m_BattleCountDown;

    #endregion



    #region Unity Callback

    public override void OnInitialize()
    {
        base.OnInitialize();
        m_StateMachine = new StateMachine<E_STATE>();
        var sceneEntering = new State<E_STATE>(E_STATE.SCENE_ENTERING);
        m_StateMachine.AddState(sceneEntering);
        sceneEntering.m_OnStart += OnStartSceneEntering;

        var countDown = new State<E_STATE>(E_STATE.COUNT_DOWN);
        m_StateMachine.AddState(countDown);
        countDown.m_OnStart += OnStartCountDown;

        var battleStart = new State<E_STATE>(E_STATE.BATTLE_START);
        m_StateMachine.AddState(battleStart);
        battleStart.m_OnStart += OnStartBattleStart;

        var battle = new State<E_STATE>(E_STATE.BATTLE);
        m_StateMachine.AddState(battle);
        battle.m_OnStart += OnStartBattle;
        battle.m_OnUpdate += OnUpdateBattle;
        battle.m_OnLateUpdate += OnLateUpdateBattle;
        battle.m_OnFixedUpdate += OnFixedUpdateBattle;
        battle.m_OnEnd += OnEndBattle;

        var battleEnd = new State<E_STATE>(E_STATE.BATTLE_END);
        m_StateMachine.AddState(battleEnd);
        battleEnd.m_OnStart += OnStartBattleEnd;

        var connectError = new State<E_STATE>(E_STATE.CONNECT_ERROR);
        m_StateMachine.AddState(connectError);
        connectError.m_OnStart += OnStartConnectError;

        m_StateMachine.Goto(E_STATE.SCENE_ENTERING);
    }

    public override void OnFinalize()
    {
        m_StateMachine.OnFinalize();
        m_StateMachine = null;

        if (m_SelfHandle)
        {
            m_SelfHandle.OnFinalize();
            m_SelfHandle = null;
        }

        if (m_OpponentHandle)
        {
            m_OpponentHandle.OnFinalize();
            m_OpponentHandle = null;
        }

        if (m_Plate)
        {
            m_Plate.OnFinalize();
            m_Plate = null;
        }

        if (m_CountDownTimer != null)
        {
            m_CountDownTimer.DestroyTimer();
            m_CountDownTimer = null;
        }

        if (m_BattleTimer != null)
        {
            m_BattleTimer.DestroyTimer();
            m_BattleTimer = null;
        }

        base.OnFinalize();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        m_StateMachine.OnUpdate();

        var state = m_StateMachine.GetCurrentState();
        if (state != null)
        {
            //Debug.Log("Current;" + state.m_Key);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BaseSceneManager.Instance.LoadScene(BaseSceneManager.E_SCENE.TITLE);
            Debug.Log("タイトルに戻ります");
        }
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
        m_Plate = Instantiate(m_PlatePrefab);
        m_SelfHandle = Instantiate(m_SelfHandlePrefab);
        m_OpponentHandle = Instantiate(m_OpponentHandlePrefab);

        m_SelfHandle.OnInitialize();
        m_OpponentHandle.OnInitialize();
        m_Plate.OnInitialize();

        m_StateMachine.Goto(E_STATE.COUNT_DOWN);
        Debug.Log("シーン遷移");
    }

    #endregion



    #region Count Down

    private void OnStartCountDown()
    {
        m_CountDown = m_CountDownTime;
        m_CountDownTimer = Timer.CreateTimer(E_TIMER_TYPE.SCALED_TIMER, 1f, m_CountDownTime);
        m_CountDownTimer.SetIntervalCallBack(OnIntervalCountDown);
        m_CountDownTimer.SetTimeoutCallBack(OnTimeoutCountDown);
        TimerManager.Instance.RegistTimer(m_CountDownTimer);
    }

    private void OnIntervalCountDown()
    {
        m_CountDown--;
        m_Text.text = m_CountDown.ToString();
    }

    private void OnTimeoutCountDown()
    {
        m_StateMachine.Goto(E_STATE.BATTLE_START);
        m_Text.text = "<color=#ff0000>BATTLE START!</color>";
    }

    #endregion



    #region Battle Start

    private void OnStartBattleStart()
    {
        m_StateMachine.Goto(E_STATE.BATTLE);
    }

    #endregion



    #region Battle

    private void OnStartBattle()
    {
        m_BattleCountDown = m_BattleTime;
        m_BattleTimer = Timer.CreateTimer(E_TIMER_TYPE.SCALED_TIMER, 1f, m_BattleTime);
        m_BattleTimer.SetIntervalCallBack(OnIntervalBattle);
        m_BattleTimer.SetTimeoutCallBack(OnTimeoutBattle);
        TimerManager.Instance.RegistTimer(m_BattleTimer);

        // プレートの投げ入れ
        if (NetproNetworkManager.Instance.IsMasterClient)
        {
            var rad = UnityEngine.Random.Range(60, 120) * Mathf.Deg2Rad;
            var sign = UnityEngine.Random.Range(0, 2) * 2 - 1;
            rad *= sign;
            var x = 100 * Mathf.Cos(rad);
            var z = 100 * Mathf.Sin(rad);
            Vector3 force = new Vector3(x, 0.0f, z);
            m_Plate.InitPlate(m_StartPlatePosition, force);
        }
    }

    private void OnIntervalBattle()
    {
        m_BattleCountDown--;
        m_Text.text = m_BattleCountDown.ToString();
    }

    private void OnTimeoutBattle()
    {
        m_StateMachine.Goto(E_STATE.BATTLE_END);
    }

    private void OnUpdateBattle()
    {
        ProcessReceivedData();

        m_SelfHandle.OnUpdate();
        m_OpponentHandle.OnUpdate();
        m_Plate.OnUpdate();
    }

    private void OnLateUpdateBattle()
    {
        m_SelfHandle.OnLateUpdate();
        m_OpponentHandle.OnLateUpdate();
        m_Plate.OnLateUpdate();
    }

    private void OnFixedUpdateBattle()
    {
        m_SelfHandle.OnFixedUpdate();
        m_OpponentHandle.OnFixedUpdate();
        m_Plate.OnFixedUpdate();
    }

    private void OnEndBattle()
    {
    }

    #endregion



    #region Battle End

    private void OnStartBattleEnd()
    {
        BaseSceneManager.Instance.LoadScene(BaseSceneManager.E_SCENE.TITLE);
        Debug.Log("バトル終了");
    }

    #endregion



    #region Connect Error

    private void OnStartConnectError()
    {
        BaseSceneManager.Instance.LoadScene(BaseSceneManager.E_SCENE.TITLE);
        Debug.LogWarning("通信が断絶されました。");
    }

    #endregion



    #region Connection Process

    /// <summary>
    /// 受信したデータを捌くための処理
    /// </summary>
    private void ProcessReceivedData()
    {
        var networkM = NetproNetworkManager.Instance;
        if (networkM.IsReceiveFailed(E_PROTOCOL_TYPE.UDP | E_PROTOCOL_TYPE.TCP))
        {
            m_StateMachine.Goto(E_STATE.CONNECT_ERROR);
            return;
        }

        if (networkM.UdpClient != null)
        {
            while (networkM.UdpClient.IsRemainReceivedData())
            {
                ProcessSwitchingData(networkM.ReceiveUdp());
            }
        }

        if (networkM.TcpClient != null)
        {
            while (networkM.TcpClient.IsRemainReceivedData())
            {
                ProcessSwitchingData(networkM.ReceiveTcp());
            }
        }
    }

    /// <summary>
    /// 受信したデータを各処理に振り分ける処理
    /// </summary>
    /// <param name="data"></param>
    private void ProcessSwitchingData(object data)
    {
        if (data == null) return;

        if (data is SyncHandleData handleData)
        {
            OnReceivedSyncHandleData(handleData);
        }
        else if (data is SyncPlateData plateData)
        {
            OnReceivedSyncPlateData(plateData);
        }
    }

    /// <summary>
    /// ハンドルデータを受け取った時の処理
    /// </summary>
    private void OnReceivedSyncHandleData(SyncHandleData handleData)
    {
        if (m_OpponentHandle != null)
        {
            m_OpponentHandle.ApplySyncHandleData(handleData);
        }
    }

    /// <summary>
    /// プレートデータを受け取った時の処理
    /// </summary>
    private void OnReceivedSyncPlateData(SyncPlateData plateData)
    {
        if (m_Plate != null)
        {
            m_Plate.ApplySyncPlateData(plateData);
        }
    }

    #endregion
}
