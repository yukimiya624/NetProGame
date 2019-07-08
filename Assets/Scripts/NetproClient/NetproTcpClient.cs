using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// TcpClientの送受信を簡易にしたクラス
/// </summary>
public class NetproTcpClient : NetproClientBase
{
    private StreamReader m_StreamReader;
    private StreamWriter m_StreamWriter;



    /// <summary>
    /// TcpClient本体。
    /// </summary>
    public TcpClient TcpClient { get; private set; }



    /// <summary>
    /// コンストラクタ。
    /// </summary>
    /// <param name="tcpClient">TcpClient本体</param>
    public NetproTcpClient(TcpClient tcpClient)
    {
        TcpClient = tcpClient;

        var networkStream = TcpClient.GetStream();
        m_StreamReader = new StreamReader(networkStream, Encoding.UTF8);
        m_StreamWriter = new StreamWriter(networkStream, Encoding.UTF8);
    }



    /// <summary>
    /// 接続を終了する。
    /// </summary>
    public override void EndClient()
    {
        Debug.LogError("TCP END!");
        if (OnReceive != null)
        {
            OnReceive = null;
        }

        if (m_StreamReader != null)
        {
            m_StreamReader.Close();
            m_StreamReader = null;
        }

        if (m_StreamWriter != null)
        {
            m_StreamWriter.Close();
            m_StreamWriter = null;
        }

        if (m_ReceiveThread != null)
        {
            m_ReceiveThread.Abort();
            m_ReceiveThread = null;
        }

        if (TcpClient != null)
        {
            TcpClient.Close();
            TcpClient = null;
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
        if (TcpClient == null)
        {
            Debug.LogError("TcpClientがありません。");
            return;
        }

        try
        {
            var sendData = string.Format("{0}{1}{0}", DATA_SPLITTER, data);
            m_StreamWriter.WriteLine(sendData);
            m_StreamWriter.Flush();
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
        while (!TcpClient.Connected) ;

        while (true)
        {
            try
            {
                var str = m_StreamReader.ReadLine();
                m_StringBuilder.Append(str);
                var data = m_StringBuilder.ToString().Trim();

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
            catch (ObjectDisposedException ode)
            {
                EndClient();
            }
            catch (SocketException se)
            {
                EndClient();
            }
        }
    }
}
