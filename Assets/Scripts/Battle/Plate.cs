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


    /// <summary>
    /// 初期化処理
    /// </summary>
    public override void OnInitialize()
    {
        base.OnInitialize();
    }

    /// <summary>
    /// 終了処理
    /// </summary>
    public override void OnFinalize()
    {
        Debug.Log("削除されました");

        if (m_GoalTimer != null)
        {
            m_GoalTimer.DestroyTimer();
        }

        base.OnFinalize();
    }

    //ゴールラインに触れると3.5秒後にゴールを決められた方のフィールドにプレートが現れる。
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "ownGoal")
        {
            m_GoalTimer = Timer.CreateTimeoutTimer(E_TIMER_TYPE.SCALED_TIMER, 3.5f, ()=> {
                m_Rigidbody.transform.position = new Vector3(m_StartPosition.x, m_StartPosition.y, m_StartPosition.z - 100);
                m_Rigidbody.velocity = Vector3.zero;
            });
            TimerManager.Instance.RegistTimer(m_GoalTimer);
        }
        if (other.gameObject.name == "enemyGoal")
        {
            m_GoalTimer = Timer.CreateTimeoutTimer(E_TIMER_TYPE.SCALED_TIMER, 3.5f, () => {
                m_Rigidbody.transform.position = new Vector3(m_StartPosition.x, m_StartPosition.y, m_StartPosition.z + 100);
                m_Rigidbody.velocity = Vector3.zero;
            });
            TimerManager.Instance.RegistTimer(m_GoalTimer);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("CollisionEnter");
        switch(collision.gameObject.tag)
        {
            case TagName.SelfHandle:
                Debug.Log("Collide Handle");
                var sendData = new SyncPlateData(2, -m_Rigidbody.position, -m_Rigidbody.velocity);
                NetproNetworkManager.Instance.SendTcp(sendData, null);
                break;
        }
    }

    public void InitPlate(Vector3 pos, Vector3 force)
    {
        m_StartPosition = pos;
        transform.position = pos;

        m_Rigidbody.AddForce(force, ForceMode.Impulse);

        var sendData = new SyncPlateData(2, -m_Rigidbody.position, -m_Rigidbody.velocity);
        NetproNetworkManager.Instance.SendTcp(sendData, null);
    }

    public void ApplySyncPlateData(SyncPlateData data)
    {
        m_Rigidbody.transform.position = data.pos;
        m_Rigidbody.velocity = data.vel;
    }
}
