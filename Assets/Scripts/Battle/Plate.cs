using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレートオブジェクト。
/// Created by Kaito Takizawa
/// </summary>
public class Plate : ControllableMonoBehavior
{
    #region Field Inspector
#pragma warning disable 649

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
    /// プレート本体のコライダ
    /// </summary>
    [SerializeField]
    private Collider m_PlateCollider;

    /// <summary>
    /// プレートの見た目に近い平たいコライダ
    /// </summary>
    [SerializeField]
    private Collider m_PlateBoxCollider;

    /// <summary>
    /// 地面に触れるためのコライダ
    /// </summary>
    [SerializeField]
    private Collider m_PlateTouchCollider;

    /// <summary>
    /// 投げ入れ時のコールバック制御
    /// </summary>
    [SerializeField]
    private PlateThrownColliderController m_PlateThrownColliderController;

#pragma warning restore 649
    #endregion



    /// <summary>
    /// ゴールした時のタイマー
    /// </summary>
    private Timer m_GoalTimer;

    /// <summary>
    /// ゴールフラグ
    /// </summary>
    private bool m_IsGoal;



    /// <summary>
    /// プレートID
    /// </summary>
    public int PlateId { get; set; }



    /// <summary>
    /// 初期化処理
    /// </summary>
    public override void OnInitialize()
    {
        base.OnInitialize();
        m_PlateThrownColliderController.SetGroundTouchCallback(OnTriggerEnterGround);

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

    private void OnTriggerEnterGround()
    {
        SetRigidbodyMode(true);
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
                Debug.Log(NetproNetworkManager.Instance.IsMasterClient);

                BattleManager.Instance.ShowOwnGoalText();
                BattleManager.Instance.GetOpponentPoint();

                SetDisplay(false);
                SendGoalData();
                m_GoalTimer = Timer.CreateTimeoutTimer(E_TIMER_TYPE.SCALED_TIMER, 3.5f, () =>
                {
                    BattleManager.Instance.HideGoalText();
                    BattleManager.Instance.ThrowPlate(this, UnityEngine.Random.Range(0, 2) == 0, true);
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
        m_PlateTouchCollider.enabled = !isEnable;
        m_PlateCollider.enabled = isEnable;
        m_Rigidbody.useGravity = !isEnable;

        if (isEnable)
        {
            m_Rigidbody.MoveRotation(Quaternion.Euler(0, 0, 0));
            var pos = m_Rigidbody.position;
            pos.y = 0;
            m_Rigidbody.position = pos;
            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        }
        else
        {
            m_Rigidbody.constraints = RigidbodyConstraints.None;
        }
    }
}
