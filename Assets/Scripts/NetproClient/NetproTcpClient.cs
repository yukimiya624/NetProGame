﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// TcpClientの送受信を簡易にしたクラス。
/// Created by Sho Yamagami.
/// </summary>
public class NetproTcpClient : NetproClientBase
{
    /// <summary>
    /// 受信ストリーム。
    /// </summary>
    private StreamReader m_StreamReader;

    /// <summary>
    /// 送信ストリーム。
    /// </summary>
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
        base.EndClient();

        Debug.Log("NetproTcpClient : EndClient");

        if (m_StreamReader != null)
        {
            try
            {
                m_StreamReader.Close();
            }
            catch (ObjectDisposedException)
            {

            }
            finally
            {
                m_StreamReader = null;
            }
        }

        if (m_StreamWriter != null)
        {
            try
            {
                m_StreamWriter.Close();
            }
            catch (ObjectDisposedException)
            {

            }
            finally
            {
                m_StreamWriter = null;
            }
        }

        if (TcpClient != null)
        {
            try
            {
                TcpClient.Close();
            }
            finally
            {
                TcpClient = null;
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
        if (TcpClient == null)
        {
            Debug.LogError("NetproTcpClient : TcpClientがありません。");
            EventUtility.SafeInvokeAction(failedSendCallback, null);
            return;
        }

        try
        {
            var sendData = string.Format("{0}{1}{0}", DATA_SPLITTER, data);
            m_StreamWriter.WriteLine(sendData);
            m_StreamWriter.Flush();
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
        while (!TcpClient.Connected) ;

        while (true)
        {
            String str = null;
            try
            {
                str = m_StreamReader.ReadLine();
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
            catch(IOException ioe)
            {
                m_ErrorQueue.Enqueue(new ErrorData("エラーが発生しました。", ioe));
                IsReceiveFailed = true;
            }

            if (!IsReceiveFailed)
            {
                StockReceiveString(str);
                EventUtility.SafeInvokeAction(OnReceive);
            }
        }
    }
}
