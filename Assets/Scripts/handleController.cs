using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class handleController : MonoBehaviour
{
    //public GameObject ground;

    [Range(0.0f, 50.0f)] public float MaxSpeed = 10.0f;
    public Vector2 MovableRangeX = new Vector2(float.NegativeInfinity, float.PositiveInfinity);
    public Vector2 MovableRangeZ = new Vector2(float.NegativeInfinity, float.PositiveInfinity);

    [Range(0.0f, 10000.0f)] public float SpringStiffness = 1000.0f;
    [Range(0.0f, 10000.0f)] public float VelocityDamper = 100.0f;
    [Range(0.0f, 10000.0f)] public float DestinationDamper = 10.0f;

    private Vector3 destination;
    private new Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        this.rb = this.GetComponent<Rigidbody>();
        this.rb.isKinematic = false;
        this.destination = this.rb.position;
    }

    // Update is called once per frame
    void Update()
    {
        this.SetDestination(Input.mousePosition);
        var sendData = new SyncHandleData(0, rb.position);
    }

    void SetDestination(Vector3 screenPoint)
    {
        Vector2 playerScreenPosition = Input.mousePosition;

        playerScreenPosition.x = Mathf.Clamp(playerScreenPosition.x, 0.0f, Screen.width);
        playerScreenPosition.y = Mathf.Clamp(playerScreenPosition.y, 0.0f, Screen.height);

        Camera gameCamera = Camera.main;
        Ray pointToRay = gameCamera.ScreenPointToRay(playerScreenPosition);

        RaycastHit hitInfo = new RaycastHit();

        //Rayが当たった(地面がある)時
        if (Physics.Raycast(pointToRay, out hitInfo))
        {
            //地面から0.01fだけ浮かせる;
            float x = hitInfo.point.x;
            float y = hitInfo.point.y + 0.01f;
            float z = hitInfo.point.z;

            Vector3 place = new Vector3(x, y, z);

            this.destination = place;

        }
    }

    private void FixedUpdate()
    {
        // ハンドルを目標地点(マウス位置)に引きつける力をかける
        this.rb.AddForce(this.GetSpringForce());
    }

    private Vector3 GetSpringForce()
    {
        var velocity = this.rb.velocity; //速度
        var speed = velocity.magnitude; //速さ
        var velocityDirection = speed < 1E-5f ? Vector3.zero : velocity / speed;
        var relativePosition = this.destination - this.rb.position;
        var sqrDistance = relativePosition.sqrMagnitude;

        //目標地点との距離に比例する復元力
        var springForce = relativePosition * this.SpringStiffness;

        //ハンドルの速度に比例する抵抗力。速度の過度な上昇を抑える。
        var velocityDamperForceMagnitude = this.VelocityDamper * speed;

        //目標地点とハンドルの距離の2乗に逆比例する抵抗力。目標地点へ到着する際のブレーキ。
        var destinationDamperForceMagnitude = this.DestinationDamper / sqrDistance;

        //ハンドルの運動量の大きさ * 秒間フレーム数
        var momentumThreshold = (speed * this.rb.mass) / Time.fixedDeltaTime;

        var damperForceMagnitude = Mathf.Min(velocityDamperForceMagnitude + destinationDamperForceMagnitude, momentumThreshold);

        var force = springForce - (damperForceMagnitude * velocityDirection); ;
        return force;
    }

}
