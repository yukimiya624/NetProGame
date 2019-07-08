using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;

public class NetproUdpClient : INetproClient
{
    private static readonly string DATA_SPLITTER = "::=::";
    private static readonly string RECEIVE_DATA_MATCHER = string.Format("{0}{1}{0}", DATA_SPLITTER, ".*?");

    private IPEndPoint m_OpponentEndPoint;
    private Thread m_ReceiveThread;
    private StringBuilder m_StringBuilder = new StringBuilder();
    private Queue<string> m_ReceiveQueue = new Queue<string>();

    private object m_SyncObject = new object();

    /// <summary>
    /// UdpClient本体
    /// </summary>
    public UdpClient UdpClient { get; private set; }

    /// <summary>
    /// 受信時コールバック
    /// </summary>
    public Action OnReceive { get; set; }

    public NetproUdpClient(IPAddress opponentAddress, int port)
    {
        // UdpClientのコンストラクタで「自身の待ち受けポート」を指定する
        // これを指定しないとReceiveでちゃんと受信してくれない
        UdpClient = new UdpClient(port);
        m_OpponentEndPoint = new IPEndPoint(opponentAddress, port);
    }

    ~NetproUdpClient()
    {
        End();
    }

    /// <summary>
    /// 受け付けを開始する
    /// </summary>
    public void Start()
    {
        if (m_ReceiveThread != null && m_ReceiveThread.IsAlive)
        {
            m_ReceiveThread.Abort();
            m_ReceiveThread = null;
        }

        m_ReceiveThread = new Thread(Receive);
        m_ReceiveThread.Start();
    }

    /// <summary>
    /// 接続を終了する。
    /// </summary>
    public  void End()
    {
        Debug.LogError("UDP END!");
        if (OnReceive != null)
        {
            OnReceive = null;
        }

        if (m_ReceiveThread != null)
        {
            m_ReceiveThread.Abort();
            m_ReceiveThread = null;
        }

        if (UdpClient != null)
        {
            UdpClient.Close();
            UdpClient = null;
        }

        if (m_StringBuilder != null)
        {
            m_StringBuilder.Clear();
            m_StringBuilder = null;
        }
    }

    /// <summary>
    /// データを送信する
    /// </summary>
    public void SendData(string data, Action failedSendCallback)
    {
        if (UdpClient == null)
        {
            Debug.LogError("UdpClientがありません。");
            return;
        }

        try
        {
            var sendData = string.Format("{0}{1}{0}", DATA_SPLITTER, data);
            var sendBytes = Encoding.UTF8.GetBytes(sendData);
            UdpClient.Send(sendBytes, sendBytes.Length, m_OpponentEndPoint);
        }
        catch (ArgumentException ae)
        {
            Debug.LogError("送信データがnullです。");
            Debug.LogException(ae);
            EventUtility.SafeInvokeAction(failedSendCallback);
        }
        catch (SocketException se)
        {
            Debug.LogError("エラーが発生しました。");
            Debug.LogException(se);
            EventUtility.SafeInvokeAction(failedSendCallback);
        }
        catch (ObjectDisposedException ode)
        {
            Debug.LogError("ソケットが閉じられました。");
            Debug.LogException(ode);
            EventUtility.SafeInvokeAction(failedSendCallback);
        }
    }

    /// <summary>
    /// 受信したデータを取り出す
    /// </summary>
    public string GetReceivedData()
    {
        if (m_ReceiveQueue == null || m_ReceiveQueue.Count < 1)
        {
            return null;
        }

        string data = null;
        lock (m_SyncObject)
        {
            data = m_ReceiveQueue.Dequeue();
        }

        return data;
    }

    /// <summary>
    /// 受信データが残っているかどうか
    /// </summary>
    public bool IsRemainReceivedData()
    {
        return m_ReceiveQueue != null && m_ReceiveQueue.Count > 0;
    }

    /// <summary>
    /// 受信データを遅延評価で全取得する
    /// </summary>
    public IEnumerable GetAllReceivedData()
    {
        while (IsRemainReceivedData())
        {
            yield return GetReceivedData();
        }
    }

    private void Receive()
    {
        while (true)
        {
            IPEndPoint remoteEp = null;
            var receiveData = UdpClient.Receive(ref remoteEp);

            var str = Encoding.UTF8.GetString(receiveData);
            m_StringBuilder.Append(str);
            var data = m_StringBuilder.ToString().Trim();

            Debug.Log("str : " + str + ", data : " + data);
            if (!string.IsNullOrEmpty(data))
            {
                var matches = Regex.Matches(data, RECEIVE_DATA_MATCHER);
                foreach (var m in matches)
                {
                    lock (m_SyncObject)
                    {
                        m_ReceiveQueue.Enqueue(m.ToString().Replace(DATA_SPLITTER, "").Trim());
                    }
                }

                m_StringBuilder.Remove(0, m_StringBuilder.Length);
                m_StringBuilder.Append(Regex.Replace(data, RECEIVE_DATA_MATCHER, ""));

                if (matches.Count > 0)
                {
                    EventUtility.SafeInvokeAction(OnReceive);
                }
            }
        }
    }
}
