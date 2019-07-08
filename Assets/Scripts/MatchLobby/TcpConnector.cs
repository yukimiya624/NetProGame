using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;

public class TcpConnector : MonoBehaviour
{
    [SerializeField]
    private InputField m_InputField;

    [SerializeField]
    private Button m_OpenServerButton;

    [SerializeField]
    private Button m_ConnectServerButton;

    private NetproTcpClient m_Client;

    private void Start()
    {
        m_OpenServerButton.onClick.AddListener(OnClickOpenServer);
        m_ConnectServerButton.onClick.AddListener(OnClickConnectServer);
    }

    private void OnClickOpenServer()
    {
        var addr = m_InputField.text;
        var ipAddr = IPAddress.Parse(addr);
        NetproNetworkManager.Instance.OpenTcpServer(ipAddr, 2059, OnAcceptTcpClient);
        Debug.Log("Wait connect... as " + ipAddr);
    }

    private void OnClickConnectServer()
    {
        var addr = m_InputField.text;
        var ipAddr = IPAddress.Parse(addr);
        NetproNetworkManager.Instance.ConnectTcpServer(ipAddr, 2059, OnConnectTcpClient);
        Debug.Log("Connect server... to " + ipAddr);
    }

    private void OnAcceptTcpClient(IAsyncResult result)
    {
        var listener = (TcpListener)result.AsyncState;
        var tcpClient = listener.EndAcceptTcpClient(result);
        listener.Stop();

        m_Client = new NetproTcpClient(tcpClient);
        m_Client.OnReceive += OnReceive;
        m_Client.StartReceive();

        Debug.Log("Connect!");
        m_Client.SendData("Hello I'm " + Dns.GetHostName(), null);
    }

    private void OnConnectTcpClient(IAsyncResult result)
    {
        m_Client = new NetproTcpClient((TcpClient) result.AsyncState);
        m_Client.OnReceive += OnReceive;
        m_Client.StartReceive();

        Debug.Log("Connect!");
        m_Client.SendData("Hello I'm " + Dns.GetHostName(), null);
    }

    private void OnReceive()
    {
        foreach (var a in m_Client.GetAllReceivedData())
        {
            Debug.LogWarning(a);
        }
    }
}
