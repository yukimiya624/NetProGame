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

/// <summary>
/// UdpClientの送受信を簡易にしたクラス。
/// Created by Sho Yamagami.
/// </summary>
public class NetproUdpClient : NetproClientBase
{
    /// <summary>
    /// 通信相手のエンドポイント。
    /// </summary>
    private IPEndPoint m_OpponentEndPoint;

    /// <summary>
    /// UdpClient本体。
    /// </summary>
    public UdpClient UdpClient { get; private set; }

    /// <summary>
    /// コンストラクタ。
    /// </summary>
    /// <param name="opponentAddress">通信相手のIPv4アドレス</param>
    /// <param name="selfPort">自身の待ち受けポート</param>
    /// <param name="opponentPort">相手の待ち受けポート</param>
    public NetproUdpClient(IPAddress opponentAddress, int selfPort, int opponentPort)
    {
        // UdpClientのコンストラクタで「自身の待ち受けポート」を指定する
        // これを指定しないとReceiveでちゃんと受信してくれない
        UdpClient = new UdpClient(selfPort);
        m_OpponentEndPoint = new IPEndPoint(opponentAddress, opponentPort);
    }

    /// <summary>
    /// 接続を終了する。
    /// </summary>
    public override void EndClient()
    {
        base.EndClient();

        Debug.Log("NetproUdpClient : EndClient");

        if (UdpClient != null)
        {
            try
            {
                UdpClient.Close();
            }
            finally
            {
                UdpClient = null;
            }
        }
    }

    /// <summary>
    /// 文字列を送信する。
    /// </summary>
    /// <param name="data">送信したい文字列</param>
    /// <param name="failedSendCallback">送信に失敗した時のコールバック</param>
    public override void SendData(string data, Action<Exception> failedSendCallback)
    {
        if (UdpClient == null)
        {
            Debug.LogError("NetproUdpClient : UdpClientがありません。");
            EventUtility.SafeInvokeAction(failedSendCallback, null);
            return;
        }

        try
        {
            var sendData = string.Format("{0}{1}{0}", DATA_SPLITTER, data);
            var sendBytes = Encoding.UTF8.GetBytes(sendData);
            UdpClient.Send(sendBytes, sendBytes.Length, m_OpponentEndPoint);
        }
        catch (Exception e)
        {
            EventUtility.SafeInvokeAction(failedSendCallback, e);
        }
    }

    /// <summary>
    /// 受信スレッドでループさせるメソッド。
    /// </summary>
    protected override void ReceiveWork()
    {
        while (true)
        {
            try
            {
                IPEndPoint remoteEp = null;
                var receiveData = UdpClient.Receive(ref remoteEp);
                var str = Encoding.UTF8.GetString(receiveData);
                StockReceiveString(str);
            }
            catch (ObjectDisposedException ode)
            {
                m_ErrorQueue.Enqueue(new ErrorData("ソケットが閉じられました。", ode));
                IsReceiveFailed = true;
            }
            catch (SocketException se)
            {
                m_ErrorQueue.Enqueue(new ErrorData("エラーが発生しました。", se));
                IsReceiveFailed = true;
            }
        }
    }
}
