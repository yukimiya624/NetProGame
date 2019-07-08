using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private enum E_State
    {
        ROOM,
        BATTLE,
    }

    public static GameManager Instance { get; private set; }


    [SerializeField]
    private GameObject m_SelfObj;

    [SerializeField]
    private GameObject m_OpponentObj;

    [SerializeField]
    private bool m_IsTransition;

    [SerializeField]
    private E_State m_State;

    private void Awake()
    {
        if (Instance)
        {
            Debug.LogWarning("GameManager is duplicate.");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        m_IsTransition = false;
        m_State = E_State.ROOM;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        switch (m_State)
        {
            case E_State.ROOM:
                UpdateOnRoom();
                break;
            case E_State.BATTLE:
                UpdateOnBattle();
                break;
        }
    }

    private void UpdateOnRoom()
    {
        if (m_IsTransition)
        {
            m_State = E_State.BATTLE;
            SceneManager.LoadScene(1);
        }
    }

    public void RequestMatch(string address)
    {
        if (!m_IsTransition)
        {
            NetproNetworkManager.Instance.RequestMatch(address, OnMatchGameManager, OnFailedMatchRequest);
        }
    }

    private void OnMatchGameManager()
    {
        m_IsTransition = true;
        NetproNetworkManager.Instance.UdpClient.OnReceive += OnReceive;
    }

    private void OnReceive()
    {
        Debug.LogWarning("On Receive UDP");
    }

    private void OnFailedMatchRequest()
    {
        Debug.LogError("マッチ失敗");
    }

    private void UpdateOnBattle()
    {
        if (m_SelfObj)
        {
            var x = Input.GetAxis("Horizontal");
            var y = Input.GetAxis("Vertical");

            var pos = m_SelfObj.transform.position;
            pos += new Vector3(x, y, 0);
            m_SelfObj.transform.position = pos;

            // 座標を送信する
            SendPosition(pos);
        }

        if (m_OpponentObj)
        {
            // 座標を受信する
            var pos = ReceivePosition();
            if (pos == null)
            {
                return;
            }

            m_OpponentObj.transform.position = (Vector3)pos;
        }
    }

    private void SendPosition(Vector3 pos)
    {
        var data = JsonUtility.ToJson(pos);
        NetproNetworkManager.Instance.UdpClient.SendData(data, null);
    }

    private Vector3? ReceivePosition()
    {
        var data = NetproNetworkManager.Instance.UdpClient.GetReceivedData();

        if (data == null)
        {
            return null;
        }

        return JsonUtility.FromJson<Vector3>(data);
    }
}
