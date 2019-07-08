using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;

public class UdpConnector : MonoBehaviour
{
    [SerializeField]
    private InputField m_InputField;

    [SerializeField]
    private Button m_ReceiveUdpButton;

    [SerializeField]
    private Button m_SendUdpButton;

    private NetproUdpClient m_Client;

    private void Start()
    {
        //m_ReceiveUdpButton.onClick.AddListener(OnClickReceiveUdpClient);
        m_ReceiveUdpButton.onClick.AddListener(OnClickSendUdpClient);
        m_SendUdpButton.onClick.AddListener(OnClickSendUdpClient);
    }

    private void OnClickReceiveUdpClient()
    {
        Debug.Log("Receive UDP...");
        m_Client.OnReceive += OnReceiveData;
        m_Client.Start();
    }

    private void OnReceiveData()
    {
        foreach (var data in m_Client.GetAllReceivedData())
        {
            Debug.Log("Receive : " + data);
        }
    }

    private void OnClickSendUdpClient()
    {
        var addr = m_InputField.text;
        var ipAddr = IPAddress.Parse(addr);

        // UDPラッパーの作成
        m_Client = new NetproUdpClient(ipAddr, 2059);

        Debug.Log("Send UDP... to " + ipAddr);
        StartCoroutine(SendHello(10, new IPEndPoint(ipAddr, 2059)));

        OnClickReceiveUdpClient();
    }

    private IEnumerator SendHello(int num, IPEndPoint endPoint)
    {
        for (int i=0;i<num;i++)
        {
            var str = string.Format("Hello I'm {0} at {1}", Dns.GetHostName(), i);
            Debug.Log("Send : " + str);
            m_Client.SendData(str, null);
            yield return new WaitForSeconds(1);
        }

        Debug.Log("Send : exit");
        m_Client.SendData("exit", null);
        Debug.Log("End Send");
    }
}
