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
    /// <param name="port">ポート</param>
    public NetproUdpClient(IPAddress opponentAddress, int port)
    {
        // UdpClientのコンストラクタで「自身の待ち受けポート」を指定する
        // これを指定しないとReceiveでちゃんと受信してくれない
        UdpClient = new UdpClient(port);
        m_OpponentEndPoint = new IPEndPoint(opponentAddress, port);
    }

    /// <summary>
    /// 接続を終了する。
    /// </summary>
    public override void EndClient()
    {
        base.EndClient();

        Debug.LogError("NetproUdpClient : EndClient");

        if (UdpClient != null)
        {
            UdpClient.Close();
            UdpClient = null;
        }
    }

    /// <summary>
    /// 文字列を送信する。
    /// </summary>
    /// <param name="data">送信したい文字列</param>
    /// <param name="failedSendCallback">送信に失敗した時のコールバック</param>
    public override void SendData(string data, Action failedSendCallback)
    {
        if (UdpClient == null)
        {
            Debug.LogError("NetproUdpClient : UdpClientがありません。");
            EventUtility.SafeInvokeAction(failedSendCallback);
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
                Debug.LogError("ソケットが閉じられました。");
                Debug.LogException(ode);
                EventUtility.SafeInvokeAction(OnReceiveFailed);
            }
            catch (SocketException se)
            {
                Debug.LogError("エラーが発生しました。");
                Debug.LogException(se);
                EventUtility.SafeInvokeAction(OnReceiveFailed);
            }
        }
    }
}
