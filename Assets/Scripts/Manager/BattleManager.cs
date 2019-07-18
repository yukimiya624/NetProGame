using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
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

    [Header("Prefab")]

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

    [Header("Hopper")]

    [SerializeField]
    private Transform m_RightLeftHopper;

    [SerializeField]
    private Transform m_RightRightHopper;

    [SerializeField]
    private Transform m_LeftLeftHopper;

    [SerializeField]
    private Transform m_LeftRightHopper;

    [SerializeField]
    private float m_HopperVelocity;

    [Header("UI")]

    [SerializeField]
    private Text m_Text;

    [SerializeField]
    private Text m_OwnPointText;

    [SerializeField]
    private Text m_OpponentPointText;

    [SerializeField]
    private Text m_GoalText;

    /// <summary>
    /// 自分の持ち点
    /// </summary>
    [SerializeField]
    private int m_OwnPoint;

    /// <summary>
    /// 敵の持ち点
    /// </summary>
    [SerializeField]
    private int m_OpponentPoint;

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

    /// <summary>
    /// プレートを新しく投げ入れるバトル開始からの経過時間
    /// </summary>
    [SerializeField]
    private float[] m_NewPlateThrownTimes;

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
    private List<Plate> m_Plates;

    /// <summary>
    /// カウントダウンタイマー
    /// </summary>
    private Timer m_CountDownTimer;

    /// <summary>
    /// バトル中のタイマー
    /// </summary>
    private Timer m_BattleTimer;

    /// <summary>
    /// プレートを投げ入れるためのタイマー
    /// </summary>
    private List<Timer> m_ThrownPlateTimers;

    private int m_CountDown;

    private int m_BattleCountDown;

    #endregion



    #region Unity Callback

    public override void OnInitialize()
    {
        base.OnInitialize();

        m_Plates = new List<Plate>();

        m_StateMachine = new StateMachine<E_STATE>();
        var sceneEntering = new State<E_STATE>(E_STATE.SCENE_ENTERING);
        m_StateMachine.AddState(sceneEntering);
        sceneEntering.m_OnStart += OnStartSceneEntering;

        var countDown = new State<E_STATE>(E_STATE.COUNT_DOWN);
        m_StateMachine.AddState(countDown);
        countDown.m_OnStart += OnStartCountDown;
        countDown.m_OnUpdate += OnUpdateCountDown;

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

        if (m_Plates != null)
        {
            foreach (var p in m_Plates)
            {
                p.OnFinalize();
            }
            m_Plates = null;
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

        if (m_ThrownPlateTimers != null)
        {
            foreach (var t in m_ThrownPlateTimers)
            {
                t.DestroyTimer();
            }
            m_ThrownPlateTimers = null;
        }

        base.OnFinalize();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        m_StateMachine.OnUpdate();

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
        m_SelfHandle = Instantiate(m_SelfHandlePrefab);
        m_OpponentHandle = Instantiate(m_OpponentHandlePrefab);

        m_SelfHandle.OnInitialize();
        m_OpponentHandle.OnInitialize();

        m_StateMachine.Goto(E_STATE.COUNT_DOWN);
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

    private void OnUpdateCountDown()
    {
        ProcessReceivedData();
    }

    private void OnIntervalCountDown()
    {
        m_CountDown--;
        m_Text.text = m_CountDown.ToString();
        if (NetproNetworkManager.Instance.IsMasterClient)
        {
            SyncCountDownData data = new SyncCountDownData(m_CountDown, DateTime.Now);
            NetproNetworkManager.Instance.SendTcp(data ,null);
        }
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

        if (NetproNetworkManager.Instance.IsMasterClient)
        {
            m_ThrownPlateTimers = new List<Timer>();
            foreach (var t in m_NewPlateThrownTimes)
            {
                var timer = Timer.CreateTimeoutTimer(E_TIMER_TYPE.SCALED_TIMER, t, ThrowNewPlate);
                TimerManager.Instance.RegistTimer(timer);
                m_ThrownPlateTimers.Add(timer);
            }
        }
    }

    private void OnIntervalBattle()
    {
        m_BattleCountDown--;
        m_Text.text = m_BattleCountDown.ToString();
    }

    private void OnTimeoutBattle()
    {
        // マスタークライアントだけ通常通りゲームエンドする
        if (NetproNetworkManager.Instance.IsMasterClient)
        {
            NetproNetworkManager.Instance.SendTcp(new SyncTimeUpData());
            m_StateMachine.Goto(E_STATE.BATTLE_END);
        }
    }

    private void OnUpdateBattle()
    {
        ProcessReceivedData();

        m_SelfHandle.OnUpdate();
        m_OpponentHandle.OnUpdate();

        foreach (var p in m_Plates)
        {
            p.OnUpdate();
        }
    }

    private void OnLateUpdateBattle()
    {
        m_SelfHandle.OnLateUpdate();
        m_OpponentHandle.OnLateUpdate();

        foreach (var p in m_Plates)
        {
            p.OnLateUpdate();
        }
    }

    private void OnFixedUpdateBattle()
    {
        m_SelfHandle.OnFixedUpdate();
        m_OpponentHandle.OnFixedUpdate();

        foreach (var p in m_Plates)
        {
            p.OnFixedUpdate();
        }
    }

    public void GetOwnPoint()
    {
        m_OwnPoint++;
        m_OwnPointText.text = m_OwnPoint.ToString();
    }

    public void GetOpponentPoint()
    {
        m_OpponentPoint++;
        m_OpponentPointText.text = m_OpponentPoint.ToString();
    }

    public void ShowGoalText()
    {
        m_GoalText.enabled = true;
        m_GoalText.text = "GOAL!!!!!!!!\n敵の陣地にパドルが再出現します";
        Debug.Log("Goal");
    }

    public void ShowOwnGoalText()
    {
        m_GoalText.enabled = true;
        m_GoalText.text = "GOAL.......\n自分の陣地にパドルが再出現します";
        Debug.Log("OwnGoal");
    }

    public void HideGoalText()
    {
        m_GoalText.enabled = false;
    }

    private void OnEndBattle()
    {
    }

    #endregion



    #region Battle End

    private void OnStartBattleEnd()
    {
        GameManager.Instance.SetBattleResult(m_OwnPoint, m_OpponentPoint);
        BaseSceneManager.Instance.LoadScene(BaseSceneManager.E_SCENE.RESULT);
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



    #region Plate

    private Plate CreatePlate(int id)
    {
        var plate = Instantiate(m_PlatePrefab);
        plate.OnInitialize();
        plate.PlateId = id;
        m_Plates.Add(plate);
        return plate;
    }

    /// <summary>
    /// プレートを新規に投げ入れる
    /// </summary>
    private void ThrowNewPlate()
    {
        var plate = CreatePlate(m_Plates.Count);
        ThrowPlate(plate, UnityEngine.Random.Range(0, 2) == 0, UnityEngine.Random.Range(0, 2) == 0);
    }

    /// <summary>
    /// プレートを投げ入れる
    /// </summary>
    public void ThrowPlate(Plate plate, bool isLeft, bool isMasterSide)
    {
        Transform hopper = null;
        if (isLeft)
        {
            hopper = isMasterSide ? m_LeftRightHopper : m_LeftLeftHopper;
        }
        else
        {
            hopper = isMasterSide ? m_RightLeftHopper : m_RightRightHopper;
        }

        if (hopper == null)
        {
            Debug.LogError("ホッパーがありません。");
            return;
        }

        plate.SendThrowInData(hopper.position, hopper.forward * m_HopperVelocity);
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
        else if (data is SyncGoalData goalData)
        {
            OnReceivedSyncGoalData(goalData);
        }
        else if (data is SyncThrowInData throwInData)
        {
            OnReceivedSyncThrowInData(throwInData);
        }
        else if (data is SyncCountDownData countDownData)
        {
            OnReceiveCountDown(countDownData);
        }
        else if (data is SyncTimeUpData timeUpData)
        {
            OnReceivedSyncTimeUpData(timeUpData);
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
        if (m_Plates != null)
        {
            var plate = m_Plates.Find(p => p.PlateId == plateData.id);
            if (plate != null)
            {
                plate.ApplySyncPlateData(plateData);
            }
        }
    }

    /// <summary>
    /// ゴールデータを受け取った時の処理
    /// </summary>
    private void OnReceivedSyncGoalData(SyncGoalData goalData)
    {
        if (m_Plates != null)
        {
            var plate = m_Plates.Find(p => p.PlateId == goalData.Id);
            if (plate != null)
            {
                plate.ApplySyncGoalData(goalData);
            }
        }
    }

    /// <summary>
    /// 投げ入れデータを受け取った時の処理
    /// </summary>
    private void OnReceivedSyncThrowInData(SyncThrowInData throwInData)
    {
        if (m_Plates != null)
        {
            var plate = m_Plates.Find(p => p.PlateId == throwInData.Id);
            if (plate == null)
            {
                plate = CreatePlate(throwInData.Id);
            }

            plate.ApplySyncThrowInData(throwInData);
        }
    }



    private void OnReceiveCountDown(SyncCountDownData countDownData)
    {
        if (!NetproNetworkManager.Instance.IsMasterClient)
        {
            Debug.Log("受け取った時間："+ DateTime.Now);
            TimeSpan Lug = DateTime.Now - countDownData.SendTime;
            Debug.Log("ラグは：" + Lug.Milliseconds);
            m_Text.text = countDownData.CountDown.ToString();

            var timer = Timer.CreateTimeoutTimer(E_TIMER_TYPE.SCALED_TIMER, 1 - Lug.Milliseconds / 1000f);
            timer.SetTimeoutCallBack(OnIntervalCountDown);
            if(m_CountDown == 0)
            {
                OnTimeoutCountDown();
            } 
        }
    }

    /// <summary>
    /// タイムアップデータを受け取った時の処理
    /// </summary>
    private void OnReceivedSyncTimeUpData(SyncTimeUpData timeUpData)
    {
        // マスタークライアントではない方がタイムアップを受け取ったらバトル終了
        if (!NetproNetworkManager.Instance.IsMasterClient)
        {
            m_StateMachine.Goto(E_STATE.BATTLE_END);
        }
    }

    #endregion
}
