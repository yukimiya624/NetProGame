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
/// クライアント間の通信を簡易にしたクラス。
/// </summary>
public abstract class NetproClientBase
{
    /// <summary>
    /// 通信のデータの区切りに用いる記号列。
    /// </summary>
    protected static readonly string DATA_SPLITTER = "::=::";

    /// <summary>
    /// 通信のデータの区切りになる正規表現。
    /// </summary>
    protected static readonly string RECEIVE_DATA_MATCHER = string.Format("{0}{1}{0}", DATA_SPLITTER, ".*?");



    #region Field

    /// <summary>
    /// 受信用のスレッド。
    /// </summary>
    protected Thread m_ReceiveThread;

    /// <summary>
    /// 受信した文字列を保持しておく。
    /// </summary>
    protected StringBuilder m_StringBuilder = new StringBuilder();

    /// <summary>
    /// 受信文字列から抽出したデータのキュー。
    /// </summary>
    protected Queue<string> m_ReceiveQueue = new Queue<string>();

    /// <summary>
    /// データ同期のために用いるオブジェクト。
    /// </summary>
    protected object m_SyncObject = new object();

    #endregion



    /// <summary>
    /// 何かしら受信した時に自動的に呼び出されるコールバック。
    /// これに関数を登録しておけば、受信した時に自動的に呼び出される。
    /// </summary>
    public Action OnReceive { get; set; }



    /// <summary>
    /// デストラクタ。
    /// </summary>
    ~NetproClientBase()
    {
        EndClient();
    }



    /// <summary>
    /// 受信を開始する。
    /// </summary>
    public void StartReceive()
    {
        if (m_ReceiveThread != null && m_ReceiveThread.IsAlive)
        {
            m_ReceiveThread.Abort();
            m_ReceiveThread = null;
        }

        m_ReceiveThread = new Thread(ReceiveWork);
        m_ReceiveThread.Start();
    }

    /// <summary>
    /// 接続を終了する。
    /// </summary>
    public virtual void EndClient()
    {
        if (OnReceive != null)
        {
            OnReceive = null;
        }

        if (m_ReceiveThread != null)
        {
            m_ReceiveThread.Abort();
            m_ReceiveThread = null;
        }

        if (m_StringBuilder != null)
        {
            m_StringBuilder.Clear();
            m_StringBuilder = null;
        }
    }

    /// <summary>
    /// 文字列を送信する。
    /// </summary>
    /// <param name="data">送信したい文字列</param>
    /// <param name="failedSendCallback">送信に失敗した時のコールバック</param>
    public abstract void SendData(string data, Action failedSendCallback);

    /// <summary>
    /// 受信データを一つ取得する。
    /// </summary>
    /// <returns>受信した文字列</returns>
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
    /// 受信データが残っているかどうかを取得する。
    /// </summary>
    /// <returns>受信データが残っている場合はtrueを返す</returns>
    public bool IsRemainReceivedData()
    {
        return m_ReceiveQueue != null && m_ReceiveQueue.Count > 0;
    }

    /// <summary>
    /// 受信データを遅延評価で全取得する。
    /// </summary>
    public IEnumerable GetAllReceivedData()
    {
        while (IsRemainReceivedData())
        {
            yield return GetReceivedData();
        }
    }

    /// <summary>
    /// 受信スレッドでループさせるメソッド。
    /// </summary>
    protected abstract void ReceiveWork();

    /// <summary>
    /// 受信した文字列をストックする。
    /// 文字列をストックした時にデータが抽出できた場合は、受信コールバックを呼び出す。
    /// </summary>
    /// <param name="receivedString">受信した文字列</param>
    protected void StockReceiveString(string receivedString)
    {
        m_StringBuilder.Append(receivedString);
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
}
