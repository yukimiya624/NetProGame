﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体の進行を管理するマネージャ。
/// Created by Sho Yamagami.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// GameManagerのステート。
    /// </summary>
    public enum E_State
    {
        ROOM,
        BATTLE,
    }

    public enum SEND_PROTOCOL
    {
        UDP,
        TCP,
    }


    #region Field Inspector
#pragma warning disable 649

    /// <summary>
    /// 自分側のプレイヤーオブジェクト。
    /// </summary>
    [SerializeField]
    private GameObject m_SelfObj;

    /// <summary>
    /// 対戦相手側のプレイヤーオブジェクト。
    /// </summary>
    [SerializeField]
    private GameObject m_OpponentObj;

    /// <summary>
    /// 送受信プロトコルタイプ。
    /// </summary>
    [SerializeField]
    private SEND_PROTOCOL m_SendProtocol;

#pragma warning restore 649
    #endregion



    #region Field

    /// <summary>
    /// GameManagerのステート。
    /// </summary>
    private E_State m_State;

    /// <summary>
    /// 遷移中かどうか。
    /// これの使い方が適当なので注意。
    /// </summary>
    private bool m_IsTransition;

    #endregion



    #region Property

    /// <summary>
    /// 自身のインスタンス。
    /// シングルトンパターンのために使用。
    /// </summary>
    public static GameManager Instance { get; private set; }

    #endregion



    #region Unity Callback

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

    #endregion



    #region Match

    /// <summary>
    /// マッチリクエスト。
    /// </summary>
    /// <param name="address">通信に用いるIPv4アドレス</param>
    public void RequestMatch(string address)
    {
        if (!m_IsTransition)
        {
            NetproNetworkManager.Instance.RequestMatch(address, OnSeccessMatch, OnFailedMatchRequest);
        }
    }

    /// <summary>
    /// マッチに成功した時の処理。
    /// </summary>
    private void OnSeccessMatch()
    {
        Debug.Log("マッチ成功");
        m_IsTransition = true;
    }

    /// <summary>
    /// マッチに失敗した時の処理。
    /// </summary>
    private void OnFailedMatchRequest()
    {
        Debug.LogError("マッチ失敗");
    }

    #endregion


    /// <summary>
    /// Roomステートの時のUpdate処理。
    /// </summary>
    private void UpdateOnRoom()
    {
        if (m_IsTransition)
        {
            m_State = E_State.BATTLE;

            // Build Settings の2つ目に登録されているシーン(PreBattle)に遷移する
            // Build Settings の登録順序によって変動するので注意
            SceneManager.LoadScene(1);
            NetproNetworkManager.Instance.UdpClient.OnReceiveFailed += OnConnectFailed;
            NetproNetworkManager.Instance.TcpClient.OnReceiveFailed += OnConnectFailed;
        }
    }

    /// <summary>
    /// Battleステートの時のUpdate処理。
    /// </summary>
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

    /// <summary>
    /// 自分自身の座標をUDPで送信する。
    /// </summary>
    private void SendPosition(Vector3 pos)
    {
        var data = JsonUtility.ToJson(pos);

        NetproClientBase client = null;
        if (m_SendProtocol == SEND_PROTOCOL.UDP)
        {
            client = NetproNetworkManager.Instance.UdpClient;
        } else
        {
            client = NetproNetworkManager.Instance.TcpClient;
        }

        client.SendData(data, OnConnectFailed);
    }

    /// <summary>
    /// UDPで受信した敵の座標を取得する。
    /// </summary>
    private Vector3? ReceivePosition()
    {
        NetproClientBase client = null;
        if (m_SendProtocol == SEND_PROTOCOL.UDP)
        {
            client = NetproNetworkManager.Instance.UdpClient;
        }
        else
        {
            client = NetproNetworkManager.Instance.TcpClient;
        }

        var data = client.GetReceivedData();

        if (data == null)
        {
            return null;
        }

        return JsonUtility.FromJson<Vector3>(data);
    }

    private void OnConnectFailed()
    {
        Debug.LogWarning("通信が断絶されました。");
        NetproNetworkManager.Instance.TcpClient.EndClient();
        NetproNetworkManager.Instance.UdpClient.EndClient();
        Application.Quit();
    }
}

