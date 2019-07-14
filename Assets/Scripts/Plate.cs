using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plate : MonoBehaviour
{
    //位置座標
    private Vector3 position;

    //スクリーン座標をワールド座標に変換した位置座標
    private Vector3 screenToWorldPointPosition;

    [SerializeField]

    //開始時の座標
    private Vector3 startPosition;

    [SerializeField]
    Rigidbody rb = new Rigidbody();

    void Start()
    {
        startPosition = new Vector3(rb.position.x, rb.position.y, rb.position.z);

        Vector3 force = new Vector3(Random.Range(-100.0f,100.0f), 0.0f, Random.Range(-100.0f, 100.0f));

        rb.AddForce(force, ForceMode.Impulse);
    }

    //ゴールラインに触れると3.5秒後にゴールを決められた方のフィールドにプレートが現れる。
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "ownGoal")
        {
            StartCoroutine(DelayMethod(3.5f, () =>
            {
                rb.transform.position = new Vector3(startPosition.x, startPosition.y, startPosition.z - 100);
                rb.velocity = Vector3.zero;
            }));
        }
        if (other.gameObject.name == "enemyGoal")
        {
            StartCoroutine(DelayMethod(3.5f, () =>
            {
                rb.transform.position = new Vector3(startPosition.x, startPosition.y, startPosition.z + 100);
                rb.velocity = Vector3.zero;
            }));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "handle")
        {
            var sendData = new SyncPlateData(3,rb.position,rb.velocity);
            NetproNetworkManager.Instance.SendUdp(sendData, null);

        }
    }

    public void ApplyPositionAndVelocity(SyncPlateData data)
    {
        rb.transform.position = data.pos;
        rb.velocity = data.vel;
    }

    private IEnumerator DelayMethod(float waittime, System.Action action)
    {
        yield return new WaitForSeconds(waittime);
        action();
    }
}
