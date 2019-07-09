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
/// Created by Sho Yamagami.
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
    /// 
    /// 引数無しの関数を用意して、
    /// OnReceive += FunctionName;
    /// とすれば、何かしら受信した時に自動的にその関数が呼び出されるようになる。
    /// もしかしたら受信した瞬間に何か処理したいかもしれないので用意した。
    /// 受信データを取得したい場合は別のメソッドを呼ぶこと。
    /// </summary>
    public Action OnReceive { get; set; }

    /// <summary>
    /// 受信に失敗した時に自動的に呼び出されるコールバック。
    /// </summary>
    public Action OnReceiveFailed { get; set; }



    /// <summary>
    /// デストラクタ。
    /// </summary>
    ~NetproClientBase()
    {
        EndClient();
    }



    /// <summary>
    /// 受信を開始する。
    /// これを呼び出さないと受信できない。
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

        if (OnReceive != null)
        {
            OnReceive = null;
        }

        if (OnReceiveFailed != null)
        {
            OnReceiveFailed = null;
        }
    }

    /// <summary>
    /// 文字列を送信する。
    /// </summary>
    /// <param name="data">送信したい文字列</param>
    /// <param name="failedSendCallback">送信に失敗した時のコールバック</param>
    public abstract void SendData(string data, Action failedSendCallback);

    /// <summary>
    /// 現在溜めている受信データの数を取得する。
    /// </summary>
    public int GetReceivedDataNum()
    {
        if (m_ReceiveQueue == null)
        {
            return 0;
        }

        lock (m_SyncObject)
        {
            return m_ReceiveQueue.Count;
        }
    }

    /// <summary>
    /// 受信データが残っているかどうかを取得する。
    /// 受信データが残っている場合はtrueを返す。
    /// </summary>
    public bool IsRemainReceivedData()
    {
        return GetReceivedDataNum() > 0;
    }

    /// <summary>
    /// 現在溜めている受信データの中から最も古くに受信したデータを一つ取得する。
    /// </summary>
    public string GetReceivedData()
    {
        if (IsRemainReceivedData())
        {
            return null;
        }

        lock (m_SyncObject)
        {
            return m_ReceiveQueue.Dequeue();
        }
    }

    /// <summary>
    /// 現在溜めている全ての受信データを配列で取得する。
    /// </summary>
    public string[] GetAllReceivedDataByArray()
    {
        if (IsRemainReceivedData())
        {
            return null;
        }

        lock (m_SyncObject)
        {
            var ary = m_ReceiveQueue.ToArray();
            m_ReceiveQueue.Clear();
            return ary;
        }
    }

    /// <summary>
    /// 現在溜めている全ての受信データを遅延評価で取得する。
    /// </summary>
    public IEnumerable GetAllReceivedDataByEnumeration()
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
    /// </summary>
    /// <param name="receivedString">受信した文字列</param>
    protected void StockReceiveString(string receivedString)
    {
        m_StringBuilder.Append(receivedString);
        var data = m_StringBuilder.ToString().Trim();

        if (string.IsNullOrEmpty(data))
        {
            return;
        }

        var matches = Regex.Matches(data, RECEIVE_DATA_MATCHER);
        foreach (var m in matches)
        {
            lock (m_SyncObject)
            {
                m_ReceiveQueue.Enqueue(m.ToString().Replace(DATA_SPLITTER, "").Trim());
            }
        }

        if (matches.Count > 0)
        {
            m_StringBuilder.Clear();
            m_StringBuilder.Append(Regex.Replace(data, RECEIVE_DATA_MATCHER, ""));
        }
    }
}
