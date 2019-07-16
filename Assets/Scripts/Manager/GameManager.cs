using System.Collections;
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
        BATTLE_DISCONNECTED,
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
    private E_PROTOCOL_TYPE m_SendProtocolType;

    [SerializeField]
    private Plate m_PlatePrefab;

    [SerializeField]
    private SelfHandleController m_SelfHandlePrefab;

    [SerializeField]
    private OpponentHandleController m_OpponentHandlePrefab;

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

    [SerializeField]
    private Plate m_Plate;

    [SerializeField]
    private SelfHandleController m_SelfHandle;

    [SerializeField]
    private OpponentHandleController m_OpponentHandle;

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

            SceneManager.LoadScene("Battle");

            m_Plate = Instantiate(m_PlatePrefab);
            m_SelfHandle = Instantiate(m_SelfHandlePrefab);
            m_OpponentHandle = Instantiate(m_OpponentHandlePrefab);

            DontDestroyOnLoad(m_Plate);
            DontDestroyOnLoad(m_SelfHandle);
            DontDestroyOnLoad(m_OpponentHandle);
        }
    }

    /// <summary>
    /// Battleステートの時のUpdate処理。
    /// </summary>
    private void UpdateOnBattle()
    {
        if (NetproNetworkManager.Instance.IsReceiveFailed(E_PROTOCOL_TYPE.UDP | E_PROTOCOL_TYPE.TCP))
        {
            OnConnectFailed();
            m_State = E_State.BATTLE_DISCONNECTED;
        }

        //if (m_Plate)
        //{
        //    //var platePos = new 

        //    var PlatePos = m_Plate.transform.position;
        //    PlatePos += m_Plate.transform.position - PlatePos;
        //    m_Plate.transform.position = PlatePos;

        //    // 座標を送信する
        //    SendPosition(PlatePos);
        //}

        //if (m_OpponentObj)
        //{
        //    // 座標を受信する
        //    var pos = ReceivePosition();
        //    if (pos == null)
        //    {
        //        return;
        //    }

        //    m_OpponentObj.transform.position = (Vector3)pos;
        //}

        while (NetproNetworkManager.Instance.TcpClient.IsRemainReceivedData())
        {
            //var receivedUDPData = NetproNetworkManager.Instance.ReceiveUdp();
            var receivedTCPData = NetproNetworkManager.Instance.ReceiveTcp();


            //if (receivedUDPData == null)
            //{
            //    continue;
            //}

            //if (receivedUDPData is SyncHandleData handleData)
            //{
            //    if (m_SelfHandle != null)
            //    {
            //        if (NetproNetworkManager.Instance.IsMasterClient && handleData.id == 0)
            //        {
            //            m_SelfHandle.ApplyPosition(handleData);
            //        }

            //        if(!NetproNetworkManager.Instance.IsMasterClient && handleData.id == 1)
            //        {
            //            m_SelfHandle.ApplyPosition(handleData);
            //        }
            //    }
            //}

            if (receivedTCPData == null)
            {
                continue;
            }


            if (receivedTCPData is SyncPlateData plateData)
            {
                if (m_Plate != null)
                {
                    if (NetproNetworkManager.Instance.IsMasterClient)
                    {
                        m_Plate.ApplyPositionAndVelocity(plateData);
                    }

                    if (!NetproNetworkManager.Instance.IsMasterClient)
                    {
                        m_Plate.ApplyPositionAndVelocity(plateData);
                    }

                }
            }

            if (receivedTCPData is SyncHandleData handleData)
            {
                if (m_OpponentHandle != null)
                {
                    Debug.Log(handleData.id+":"+handleData.pos);
                    if (NetproNetworkManager.Instance.IsMasterClient && handleData.id == 0)
                    {
                        m_OpponentHandle.ApplyPosition(handleData);
                    }

                    if (!NetproNetworkManager.Instance.IsMasterClient && handleData.id == 1)
                    {
                        m_OpponentHandle.ApplyPosition(handleData);
                    }
                }
            }
        }
    
    }

    /// <summary>
    /// 自分自身の座標をUDPで送信する。
    /// </summary>
    private void SendPosition(Vector3 pos)
    {
        var data = new NetproVector3();
        data.SetVector3(pos);
        NetproNetworkManager.Instance.SendUdp(data, null);
    }

    /// <summary>
    /// UDPで受信した敵の座標を取得する。
    /// </summary>
    private Vector3? ReceivePosition()
    {
        var data = NetproNetworkManager.Instance.ReceiveUdp();

        if (data == null)
        {
            return null;
        }

        if (data is NetproVector3 pos)
        {
            return pos.GetVector3();
        }

        return null;
    }

    private void OnConnectFailed()
    {
        Debug.LogWarning("通信が断絶されました。");
        NetproNetworkManager.Instance.CloseClients();
        Application.Quit();
    }
}

