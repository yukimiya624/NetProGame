using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 自身のハンドルオブジェクト。
/// Created by Kaito Takizawa
/// </summary>
public class SelfHandleController : ControllableMonoBehavior
{
    [SerializeField, Range(0.0f, 10000.0f)]
    private float SpringStiffness = 1000.0f;

    [SerializeField, Range(0.0f, 10000.0f)]
    private float VelocityDamper = 100.0f;

    [SerializeField, Range(0.0f, 10000.0f)]
    private float DestinationDamper = 10.0f;

    /// <summary>
    /// ハンドルの左下制限座標
    /// </summary>
    [SerializeField]
    private Vector2 m_RestrictMinPos;

    /// <summary>
    /// ハンドルの右上制限座標
    /// </summary>
    [SerializeField]
    private Vector2 m_RestrictMaxPos;

    [SerializeField]
    Rigidbody m_Rigidbody;

    [SerializeField]
    private LayerMask m_GroundLayerMask;

    private Vector3 m_Destination;



    /// <summary>
    /// 初期化処理
    /// </summary>
    public override void OnInitialize()
    {
        base.OnInitialize();

        var pos = new Vector3(0, 0, -100);
        m_Rigidbody.position = pos;
        m_Rigidbody.isKinematic = false;

        m_Destination = pos;
    }

    /// <summary>
    /// 終了処理
    /// </summary>
    public override void OnFinalize()
    {
        base.OnFinalize();
    }

    /// <summary>
    /// 毎フレーム呼び出される処理
    /// </summary>
    public override void OnUpdate()
    {
        base.OnUpdate();

        SetDestination(Input.mousePosition);

        // テーブルの中心を点対象として敵の画面に投影されるため、XZを反対にする
        var posData = new Vector3(-m_Rigidbody.position.x, m_Rigidbody.position.y, -m_Rigidbody.position.z);

        SyncHandleData handleData;
        if (NetproNetworkManager.Instance.IsMasterClient)
        {
            handleData = new SyncHandleData(1, posData);
        }
        else
        {
            handleData = new SyncHandleData(0, posData);
        }

        NetproNetworkManager.Instance.SendUdp(handleData, null);
    }

    /// <summary>
    /// 固定フレームで呼び出される処理
    /// </summary>
    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        // ハンドルを目標地点(マウス位置)に引きつける力をかける
        m_Rigidbody.AddForce(GetSpringForce());
    }

    private void SetDestination(Vector3 screenPoint)
    {
        Vector2 mPos = Input.mousePosition;
        Vector3 pos = GetViewportWorldPoint(mPos.x, mPos.y, 0);

        pos.x = Mathf.Clamp(pos.x, m_RestrictMinPos.x, m_RestrictMaxPos.x);
        pos.z = Mathf.Clamp(pos.z, m_RestrictMinPos.y, m_RestrictMaxPos.y);

        m_Destination = pos;
    }

    private Vector3 GetSpringForce()
    {
        var velocity = m_Rigidbody.velocity; //速度
        var speed = velocity.magnitude; //速さ
        var velocityDirection = speed < 1E-5f ? Vector3.zero : velocity / speed;
        var relativePosition = this.m_Destination - m_Rigidbody.position;
        var sqrDistance = relativePosition.sqrMagnitude;

        //目標地点との距離に比例する復元力
        var springForce = relativePosition * this.SpringStiffness;

        //ハンドルの速度に比例する抵抗力。速度の過度な上昇を抑える。
        var velocityDamperForceMagnitude = this.VelocityDamper * speed;

        //目標地点とハンドルの距離の2乗に逆比例する抵抗力。目標地点へ到着する際のブレーキ。
        var destinationDamperForceMagnitude = this.DestinationDamper / sqrDistance;

        //ハンドルの運動量の大きさ * 秒間フレーム数
        var momentumThreshold = (speed * m_Rigidbody.mass) / Time.fixedDeltaTime;

        var damperForceMagnitude = Mathf.Min(velocityDamperForceMagnitude + destinationDamperForceMagnitude, momentumThreshold);

        return springForce - (damperForceMagnitude * velocityDirection); ;
    }

    /// <summary>
    /// ビューポート座標からワールド座標に変換する。
    /// </summary>
    /// <param name="baseHeight">無限平面の高さ</param>
    private Vector3 GetViewportWorldPoint(float x, float y, float baseHeight)
    {
        var camera = Camera.main;
        Vector3 farPos = camera.ScreenToWorldPoint(new Vector3(x, y, camera.nearClipPlane));
        Vector3 originPos = camera.transform.position;
        Vector3 dir = (farPos - originPos).normalized;

        Vector3 axis = Vector3.up;
        float h = Vector3.Dot(new Vector3(0, baseHeight, 0), axis);
        return originPos + dir * (h - Vector3.Dot(axis, originPos)) / (Vector3.Dot(axis, dir));
    }
}
