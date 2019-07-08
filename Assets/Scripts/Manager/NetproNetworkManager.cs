using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

public class NetproNetworkManager : MonoBehaviour
{
    #region Field Inspector
#pragma warning disable 649

    /// <summary>
    /// ローカルサーバを使用するかどうか
    /// </summary>
    [SerializeField]
    private bool m_UseLocalServer;

    /// <summary>
    /// ローカルサーバのURL
    /// </summary>
    [SerializeField]
    private string m_LocalServerUrl;

    /// <summary>
    /// リモートサーバのURL
    /// </summary>
    [SerializeField]
    private string m_RemoteServerUrl;

    /// <summary>
    /// P2P通信で用いるポート番号
    /// </summary>
    [SerializeField]
    private int m_P2Pport;

    [SerializeField]
    private bool m_UseSpecifiedAddress;

    [SerializeField]
    private string m_SpecifiledAddress;

#pragma warning restore 649
    #endregion



    #region Field

    private TcpListener m_TcpListener;
    private LinkedList<WebClient> m_WebClientList;

    #endregion



    #region Property

    /// <summary>
    /// 自身のインスタンス
    /// </summary>
    public static NetproNetworkManager Instance { get; private set; }

    /// <summary>
    /// クライアントPCのIPv4アドレス
    /// </summary>
    public IPAddress SelfIpAddress { get; private set; }

    /// <summary>
    /// 対戦相手のIPv4アドレス
    /// </summary>
    public IPAddress OpponentIpAddress { get; private set; }

    /// <summary>
    /// 対戦相手との通信に用いるTcpクライアント
    /// </summary>
    public NetproTcpClient TcpClient { get; private set; }

    /// <summary>
    /// 対戦相手との通信に用いるUdpクライアント
    /// </summary>
    public NetproUdpClient UdpClient { get; private set; }

    /// <summary>
    /// 対戦相手とマッチした時のコールバック
    /// </summary>
    public Action MatchCallBack { get; set; }

    #endregion



    #region Unity Callback

    private void Awake()
    {
        if (Instance)
        {
            Debug.LogWarning("NetworkManager is duplicate.");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            m_WebClientList = new LinkedList<WebClient>();
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void Start()
    {
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    private void OnApplicationFocus(bool focus)
    {

    }

    private void OnApplicationPause(bool pause)
    {

    }

    private void OnApplicationQuit()
    {
        if (m_WebClientList != null)
        {
            foreach (var client in m_WebClientList)
            {
                if (client != null) client.Dispose();
            }
            m_WebClientList.Clear();
            m_WebClientList = null;
        }

        if (m_TcpListener != null)
        {
            m_TcpListener.Stop();
            m_TcpListener = null;
        }

        if (TcpClient != null)
        {
            TcpClient.End();
            TcpClient = null;
        }

        if (UdpClient != null)
        {
            UdpClient.End();
            UdpClient = null;
        }
    }

    #endregion




    /// <summary>
    /// 自身のPCのIPv4アドレスを取得する
    /// </summary>
    private void FindSelfIpAddress()
    {
        if (m_UseSpecifiedAddress)
        {
            IPAddress ipAddress;
            if (IPAddress.TryParse(m_SpecifiledAddress, out ipAddress))
            {
                SelfIpAddress = ipAddress;
                return;
            }
        }

        IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());

        foreach (var address in addresses)
        {
            // IPv4 のみ取得する
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                SelfIpAddress = address;
                return;
            }
        }
    }

    /// <summary>
    /// 自身のPCのIPv4アドレスを取得する
    /// </summary>
    public List<IPAddress> GetSelfIpAddresses()
    {
        IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());
        var list = new List<IPAddress>();

        foreach (var address in addresses)
        {
            // IPv4 のみ取得する
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                list.Add(address);
            }
        }

        return list;
    }

    /// <summary>
    /// Tcpクライアントを接続する
    /// </summary>
    private void ConnectTcpClient(TcpClient tcpClient)
    {
        TcpClient = new NetproTcpClient(tcpClient);
        TcpClient.Start();
    }

    /// <summary>
    /// Udpクライアントを接続する
    /// </summary>
    private void ConnectUdpClient()
    {
        if (OpponentIpAddress == null)
        {
            Debug.LogError("通信相手のIPv4アドレスがありません。");
            return;
        }

        UdpClient = new NetproUdpClient(OpponentIpAddress, m_P2Pport);
        UdpClient.Start();
    }



    #region Match

    /// <summary>
    /// マッチ待機リクエストを送信する
    /// </summary>
    public void RequestMatch(Action seccessMatchCallback, Action failureRequestCallback = null)
    {
        FindSelfIpAddress();

        if (SelfIpAddress == null)
        {
            Debug.LogError("マッチ待機リクエスト失敗 自身のIPアドレスを取得できませんでした");
        }

        var matchRequestData = new MatchRequestData(SelfIpAddress.ToString());

        var url = m_UseLocalServer ? m_LocalServerUrl : m_RemoteServerUrl;
        var sendData = JsonUtility.ToJson(matchRequestData);
        var apiClient = new NetproApiClient();

        try
        {
            MatchCallBack += seccessMatchCallback;
            apiClient.UploadDataAsync(url, 5000, sendData, OnRequestMatch);
        }
        catch (ArgumentException ae)
        {
            Debug.LogError("アドレスが不正です。");
            Debug.LogException(ae);
            MatchCallBack -= seccessMatchCallback;
            EventUtility.SafeInvokeAction(failureRequestCallback);
        }
        catch (WebException we)
        {
            Debug.Log("エラーが発生しました。");
            Debug.LogException(we);
            MatchCallBack -= seccessMatchCallback;
            EventUtility.SafeInvokeAction(failureRequestCallback);
        }
        finally
        {
            apiClient.Dispose();
            apiClient = null;
        }
    }

    public void RequestMatch(string address, Action seccessMatchCallback, Action failureRequestCallback = null)
    {
        SelfIpAddress = IPAddress.Parse(address);

        if (SelfIpAddress == null)
        {
            Debug.LogError("マッチ待機リクエスト失敗 自身のIPアドレスを取得できませんでした");
        }

        var matchRequestData = new MatchRequestData(SelfIpAddress.ToString());

        var url = m_UseLocalServer ? m_LocalServerUrl : m_RemoteServerUrl;
        var sendData = JsonUtility.ToJson(matchRequestData);
        var apiClient = new NetproApiClient();

        try
        {
            MatchCallBack += seccessMatchCallback;
            apiClient.UploadDataAsync(url, 5000, sendData, OnRequestMatch);
        }
        catch (ArgumentException ae)
        {
            Debug.LogError("アドレスが不正です。");
            Debug.LogException(ae);
            MatchCallBack -= seccessMatchCallback;
            EventUtility.SafeInvokeAction(failureRequestCallback);
        }
        catch (WebException we)
        {
            Debug.Log("エラーが発生しました。");
            Debug.LogException(we);
            MatchCallBack -= seccessMatchCallback;
            EventUtility.SafeInvokeAction(failureRequestCallback);
        }
        finally
        {
            apiClient.Dispose();
            apiClient = null;
        }
    }

    /// <summary>
    /// マッチ待機リクエストコールバック
    /// </summary>
    private void OnRequestMatch(UploadDataCompletedEventArgs args)
    {
        try
        {
            var receiveData = JsonUtility.FromJson<MatchResponseData>(Encoding.UTF8.GetString(args.Result));

            if (receiveData.Status != "SUCCESS")
            {
                Debug.LogError("Request Match Error : " + Encoding.UTF8.GetString(args.Result));
                return;
            }

            if (receiveData.Message == "OPEN_SERVER")
            {
                OpenTcpServer(SelfIpAddress, m_P2Pport, OnAcceptTcpClient);
                Debug.Log("Open Server as " + SelfIpAddress);
            }
            else if (receiveData.Message == "OPEN_CLIENT")
            {
                IPAddress ipAddress;
                if (!IPAddress.TryParse(receiveData.Addr, out ipAddress))
                {
                    Debug.LogError("Request Match Error : 対戦相手のIPv4アドレスのパースに失敗しました。IPv4 : " + receiveData.Addr);
                    return;
                }

                OpponentIpAddress = ipAddress;
                ConnectTcpServer(OpponentIpAddress, m_P2Pport, OnConnectTcpClient);
                Debug.Log("Connect Server as " + OpponentIpAddress);
            }
        }
        catch (SocketException se)
        {
            Debug.LogError("マッチリクエストに失敗しました。");
            Debug.LogException(se);
        }
    }

    /// <summary>
    /// Tcpクライアント接続要求コールバック
    /// </summary>
    private void OnAcceptTcpClient(IAsyncResult result)
    {
        Debug.Log("Accept Client!");

        var listener = (TcpListener)result.AsyncState;
        var tcpClient = listener.EndAcceptTcpClient(result);
        listener.Stop();

        // 接続してきた相手のアドレスを取得する
        var ep = (IPEndPoint)tcpClient.Client.RemoteEndPoint;
        OpponentIpAddress = ep.Address;

        // 接続開始
        ConnectUdpClient();
        ConnectTcpClient(tcpClient);
        EventUtility.SafeInvokeAction(MatchCallBack);
    }

    /// <summary>
    /// Tcpクライアント接続コールバック
    /// </summary>
    private void OnConnectTcpClient(IAsyncResult result)
    {
        Debug.Log("Connect Server!");

        var tcpClient = (TcpClient)result.AsyncState;

        // 接続開始
        ConnectUdpClient();
        ConnectTcpClient(tcpClient);
        EventUtility.SafeInvokeAction(MatchCallBack);
    }

    #endregion


    public void OpenTcpServer(IPAddress address, int port, Action<IAsyncResult> asyncCallback)
    {
        var listener = new TcpListener(address, port);
        listener.Start();
        listener.BeginAcceptTcpClient(new AsyncCallback(asyncCallback), listener);
    }

    public void ConnectTcpServer(IPAddress address, int port, Action<IAsyncResult> asyncCallback)
    {
        var client = new TcpClient();
        client.BeginConnect(address, port, new AsyncCallback(asyncCallback), client);
    }
}
