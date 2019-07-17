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

    [SerializeField]
    Rigidbody m_Rigidbody;

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

        if (mPos.x < 0 || mPos.x > Screen.width || mPos.y < 0 || mPos.y > Screen.height)
        {
            return;
        }

        mPos.x = Mathf.Clamp(mPos.x, 0.0f, Screen.width);
        mPos.y = Mathf.Clamp(mPos.y, 0.0f, Screen.height);

        Camera gameCamera = Camera.main;
        Ray pointToRay = gameCamera.ScreenPointToRay(mPos);

        RaycastHit hitInfo = new RaycastHit();

        //Rayが当たった(地面がある)時
        if (Physics.Raycast(pointToRay, out hitInfo))
        {
            //地面から0.01fだけ浮かせる;
            float x = hitInfo.point.x;
            float y = hitInfo.point.y + 0.01f;
            float z = hitInfo.point.z;

            Vector3 place = new Vector3(x, y, z);

            this.m_Destination = place;
        }
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
}
