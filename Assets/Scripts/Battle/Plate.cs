using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレートオブジェクト。
/// Created by Kaito Takizawa
/// </summary>
public class Plate : ControllableMonoBehavior
{

    [SerializeField]
    private Rigidbody m_Rigidbody;

    /// <summary>
    /// 開始時の座標
    /// </summary>
    private Vector3 m_StartPosition;

    /// <summary>
    /// ゴールした時のタイマー
    /// </summary>
    private Timer m_GoalTimer;

    private bool m_InitSync;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override void OnInitialize()
    {
        base.OnInitialize();
        m_InitSync = false;
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

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        if (m_InitSync)
        {
            if (m_Rigidbody.velocity.sqrMagnitude > 0)
            {
                m_InitSync = false;
                SendSyncPlateData();
            }
        }
    }

    //ゴールラインに触れると3.5秒後にゴールを決められた方のフィールドにプレートが現れる。
    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case TagName.OwnGoal:
                m_GoalTimer = Timer.CreateTimeoutTimer(E_TIMER_TYPE.SCALED_TIMER, 3.5f, () =>
                {
                    m_Rigidbody.position = new Vector3(m_StartPosition.x, m_StartPosition.y, m_StartPosition.z - 100);
                    m_Rigidbody.velocity = Vector3.zero;
                    SendSyncPlateData();
                });
                TimerManager.Instance.RegistTimer(m_GoalTimer);
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

    public void InitPlate(Vector3 pos, Vector3 force)
    {
        m_InitSync = true;
        m_StartPosition = pos;
        m_Rigidbody.position = pos;
        m_Rigidbody.AddForce(force, ForceMode.Impulse);
    }

    private void SendSyncPlateData()
    {
        var pos = m_Rigidbody.position;
        pos.x *= -1;
        pos.z *= -1;
        var sendData = new SyncPlateData(2, pos, -m_Rigidbody.velocity);
        NetproNetworkManager.Instance.SendTcp(sendData, null);
    }

    public void ApplySyncPlateData(SyncPlateData data)
    {
        m_Rigidbody.position = data.pos;
        m_Rigidbody.velocity = data.vel;
    }
}
