using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレートオブジェクト。
/// Created by Kaito Takizawa
/// </summary>
public class Plate : ControllableMonoBehavior
{
    /// <summary>
    /// レンダラ
    /// </summary>
    [SerializeField]
    private Renderer m_Renderer;

    /// <summary>
    /// 剛体コンポーネント
    /// </summary>
    [SerializeField]
    private Rigidbody m_Rigidbody;

    /// <summary>
    /// プレートの見た目に近い平たいコライダ
    /// </summary>
    [SerializeField]
    private Collider m_PlateBoxCollider;

    /// <summary>
    /// 開始時の座標
    /// </summary>
    private Vector3 m_StartPosition;

    /// <summary>
    /// ゴールした時のタイマー
    /// </summary>
    private Timer m_GoalTimer;

    private bool m_IsGoal;

    public int PlateId { get; set; }

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override void OnInitialize()
    {
        base.OnInitialize();
        m_IsGoal = false;
        SetDisplay(false);
    }

    /// <summary>
    /// 終了処理
    /// </summary>
    public override void OnFinalize()
    {
        if (m_GoalTimer != null)
        {
            m_GoalTimer.DestroyTimer();
        }

        base.OnFinalize();
    }

    //ゴールラインに触れると3.5秒後にゴールを決められた方のフィールドにプレートが現れる。
    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case TagName.OwnGoal:
                if (m_IsGoal)
                {
                    break;
                }

                m_IsGoal = true;

                BattleManager.Instance.ShowOwnGoalText();
                BattleManager.Instance.GetOpponentPoint();

                SendGoalData();
                m_GoalTimer = Timer.CreateTimeoutTimer(E_TIMER_TYPE.SCALED_TIMER, 3.5f, () =>
                {
                    BattleManager.Instance.HideGoalText();
                    BattleManager.Instance.ThrowPlate(this, UnityEngine.Random.Range(0, 2) == 0, NetproNetworkManager.Instance.IsMasterClient);
                });
                TimerManager.Instance.RegistTimer(m_GoalTimer);
                break;

            case TagName.Goal:
                if (m_IsGoal)
                {
                    break;
                }

                m_IsGoal = true;

                BattleManager.Instance.ShowGoalText();
                BattleManager.Instance.GetOwnPoint();

                SetDisplay(false);
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnCollide(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        OnCollide(collision);
    }

    private void OnCollide(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case TagName.SelfHandle:
                SendSyncPlateData();
                break;
            case TagName.Ground:
                m_PlateBoxCollider.enabled = false;
                SetRigidbodyMode(true);
                break;
        }
    }

    /// <summary>
    /// 表示設定を変える
    /// </summary>
    public void SetDisplay(bool isEnable)
    {
        m_Renderer.enabled = isEnable;
    }

    private void SendSyncPlateData()
    {
        var pos = m_Rigidbody.position;
        pos.x *= -1;
        pos.z *= -1;
        var data = new SyncPlateData(2, pos, -m_Rigidbody.velocity);
        data.id = PlateId;
        NetproNetworkManager.Instance.SendTcp(data, null);
    }

    private void SendGoalData()
    {
        var data = new SyncGoalData();
        data.Id = PlateId;
        NetproNetworkManager.Instance.SendTcp(data, null);
    }

    public void SendThrowInData(Vector3 pos, Vector3 vel)
    {
        m_IsGoal = false;
        SetRigidbodyMode(false);
        SetDisplay(true);

        m_Rigidbody.position = pos;
        m_Rigidbody.velocity = vel;

        pos.x *= -1;
        pos.z *= -1;
        vel.x *= -1;
        vel.z *= -1;
        
        var data = new SyncThrowInData();
        data.Pos = pos;
        data.Vel = vel;
        data.Id = PlateId;

        NetproNetworkManager.Instance.SendTcp(data, null);
    }

    public void ApplySyncPlateData(SyncPlateData data)
    {
        m_Rigidbody.position = data.pos;
        m_Rigidbody.velocity = data.vel;
    }

    public void ApplySyncGoalData(SyncGoalData data)
    {
        SetRigidbodyMode(false);
        SetDisplay(false);
    }

    public void ApplySyncThrowInData(SyncThrowInData data)
    {
        BattleManager.Instance.HideGoalText();
        m_IsGoal = false;
        SetRigidbodyMode(false);
        SetDisplay(true);
        m_Rigidbody.position = data.Pos;
        m_Rigidbody.velocity = data.Vel;
    }

    /// <summary>
    /// 衝突判定や剛体処理を普通のプレートのようにするかどうか
    /// </summary>
    /// <param name="isEnable">グラウンドで滑るようにするか</param>
    public void SetRigidbodyMode(bool isEnable)
    {
        m_PlateBoxCollider.enabled = !isEnable;
        m_Rigidbody.useGravity = !isEnable;

        if (isEnable)
        {
            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            m_Rigidbody.MoveRotation(Quaternion.Euler(0, 0, 0));
        }
        else
        {
            m_Rigidbody.constraints = RigidbodyConstraints.None;
        }
    }
}
