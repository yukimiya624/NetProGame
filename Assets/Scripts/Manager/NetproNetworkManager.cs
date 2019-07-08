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
    /// リモートサーバのURL。
    /// </summary>
    [SerializeField]
    private string m_RemoteServerUrl;

    /// <summary>
    /// P2P通信で用いるポート番号。
    /// </summary>
    [SerializeField]
    private int m_P2Pport;

#pragma warning restore 649
    #endregion



    #region Field

    /// <summary>
    /// 対戦相手とマッチした時のコールバック。
    /// </summary>
    private Action m_MatchCallBack;

    /// <summary>
    /// マッチリクエストで何らかの失敗が発生した時のコールバック。
    /// </summary>
    private Action m_FailureMatchRequestCallBack;

    #endregion



    #region Property

    /// <summary>
    /// 自身のインスタンス。
    /// シングルトンパターンのために使用。
    /// </summary>
    public static NetproNetworkManager Instance { get; private set; }

    /// <summary>
    /// 通信に用いる、自身のクライアントPCのIPv4アドレス。
    /// </summary>
    public IPAddress SelfIpAddress { get; private set; }

    /// <summary>
    /// 通信に用いられている、相手のクライアントのIPv4アドレス。
    /// </summary>
    public IPAddress OpponentIpAddress { get; private set; }

    /// <summary>
    /// 対戦相手との通信に用いるTcpクライアント。
    /// 確実に通信したい場合に用いる。
    /// </summary>
    public NetproTcpClient TcpClient { get; private set; }

    /// <summary>
    /// 対戦相手との通信に用いるUdpクライアント。
    /// リアルタイム通信したい場合に用いる。
    /// </summary>
    public NetproUdpClient UdpClient { get; private set; }

    #endregion



    #region Unity Callback

    /// <summary>
    /// このインスタンスの生成時の処理。
    /// </summary>
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
        }
    }

    /// <summary>
    /// このインスタンスの破棄時の処理。
    /// </summary>
    private void OnDestroy()
    {
        CloseClients();
        Instance = null;
    }

    /// <summary>
    /// このインスタンスが生成されてから最初のフレームで呼び出される。
    /// </summary>
    private void Start()
    {
    }

    /// <summary>
    /// 毎フレーム呼び出される。
    /// </summary>
    private void Update()
    {
    }

    /// <summary>
    /// 全てのUpdate()関数が呼び出された後に毎フレーム呼び出される。
    /// </summary>
    private void LateUpdate()
    {
    }

    /// <summary>
    /// 固定間隔で呼び出される。
    /// </summary>
    private void FixedUpdate()
    {
    }

    /// <summary>
    /// gameObjectがtrueになった時に呼び出される。
    /// </summary>
    private void OnEnable()
    {

    }

    /// <summary>
    /// gameObjectがfalseになった時に呼び出される。
    /// </summary>
    private void OnDisable()
    {

    }

    /// <summary>
    /// このアプリケーションのアクティブが切り替わった時に呼び出される。
    /// </summary>
    /// <param name="isActiveFocus">このアプリケーションのウィンドウがアクティブかどうか</param>
    private void OnApplicationFocus(bool isActiveFocus)
    {

    }

    /// <summary>
    /// このアプリケーションのポーズが切り替わった時に呼び出される。
    /// </summary>
    /// <param name="isPause">このアプリケーションがポーズしているかどうか</param>
    private void OnApplicationPause(bool isPause)
    {

    }

    /// <summary>
    /// このアプリケーションが終了した時に呼び出される。
    /// </summary>
    private void OnApplicationQuit()
    {
        CloseClients();
    }

    #endregion



    /// <summary>
    /// 自身のPCのIPv4アドレスを取得する。
    /// </summary>
    /// <returns>IPv4アドレスのリスト</returns>
    public List<IPAddress> FindSelfIpAddresses()
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
    /// Tcpリスナとして、Tcpクライアントの接続を待ち受ける。
    /// </summary>
    /// <param name="address">自分自身のアドレス</param>
    /// <param name="port"></param>
    /// <param name="asyncCallback">接続完了時コールバック</param>
    public void OpenTcpServer(IPAddress address, int port, Action<IAsyncResult> asyncCallback)
    {
        var listener = new TcpListener(address, port);
        listener.Start();
        listener.BeginAcceptTcpClient(new AsyncCallback(asyncCallback), listener);
    }

    /// <summary>
    /// Tcpクライアントとして、相手のTcpリスナに接続する。
    /// </summary>
    /// <param name="address">相手のアドレス</param>
    /// <param name="port">ポート</param>
    /// <param name="asyncCallback">接続完了時コールバック</param>
    public void ConnectTcpServer(IPAddress address, int port, Action<IAsyncResult> asyncCallback)
    {
        var client = new TcpClient();
        client.BeginConnect(address, port, new AsyncCallback(asyncCallback), client);
    }

    /// <summary>
    /// Tcpクライアントを渡して、より扱いやすいNetproTcpClientを作成する。
    /// </summary>
    /// <param name="tcpClient">クライアント通信の開始時に作成したTcpクライアント</param>
    private void CreateNetproTcpClient(TcpClient tcpClient)
    {
        TcpClient = new NetproTcpClient(tcpClient);
        TcpClient.StartReceive();
    }

    /// <summary>
    /// より扱いやすいNetproUdpクライアントを作成する。
    /// </summary>
    private void CreateNetproUdpClient()
    {
        if (OpponentIpAddress == null)
        {
            Debug.LogError("通信相手のIPv4アドレスがありません。");
            return;
        }

        UdpClient = new NetproUdpClient(OpponentIpAddress, m_P2Pport);
        UdpClient.Start();
    }

    /// <summary>
    /// クライアントインスタンスを閉じる。
    /// </summary>
    private void CloseClients()
    {
        if (TcpClient != null)
        {
            TcpClient.EndClient();
            TcpClient = null;
        }

        if (UdpClient != null)
        {
            UdpClient.End();
            UdpClient = null;
        }
    }


    #region Match

    /// <summary>
    /// マッチリクエストを送る。
    /// </summary>
    /// <param name="address">マッチに用いる自身のIPv4アドレス</param>
    /// <param name="seccessMatchCallback"></param>
    /// <param name="failureRequestCallback"></param>
    public void RequestMatch(string address, Action seccessMatchCallback, Action failureRequestCallback = null)
    {
        IPAddress addr;
        if (!IPAddress.TryParse(address, out addr))
        {
            Debug.LogError("Match Request Error : アドレスのパースに失敗しました。 address : " + address);
            EventUtility.SafeInvokeAction(failureRequestCallback);
            return;
        }

        SelfIpAddress = addr;

        if (SelfIpAddress == null)
        {
            Debug.LogError("Match Request Error : 自身のIPアドレスを取得できませんでした");
            EventUtility.SafeInvokeAction(failureRequestCallback);
            return;
        }

        var matchRequestData = new MatchRequestData(SelfIpAddress.ToString());
        var sendData = JsonUtility.ToJson(matchRequestData);
        var apiClient = new NetproApiClient();

        try
        {
            m_MatchCallBack += seccessMatchCallback;
            m_FailureMatchRequestCallBack += failureRequestCallback;
            apiClient.UploadDataAsync(m_RemoteServerUrl, 5000, sendData, OnRequestMatch);
        }
        catch (ArgumentException ae)
        {
            Debug.LogError("Match Request Error :  アドレスが不正です。");
            Debug.LogException(ae);
            m_MatchCallBack -= seccessMatchCallback;
            EventUtility.SafeInvokeAction(m_FailureMatchRequestCallBack);
            m_FailureMatchRequestCallBack = null;
        }
        catch (WebException we)
        {
            Debug.Log("Match Request Error : エラーが発生しました。");
            Debug.LogException(we);
            m_MatchCallBack -= seccessMatchCallback;
            EventUtility.SafeInvokeAction(m_FailureMatchRequestCallBack);
            m_FailureMatchRequestCallBack = null;
        }
        finally
        {
            apiClient.Dispose();
            apiClient = null;
        }
    }

    /// <summary>
    /// リモートサーバにマッチリクエストを送った後のコールバック。
    /// </summary>
    /// <param name="args">リモートサーバから返ってきたデータ</param>
    private void OnRequestMatch(UploadDataCompletedEventArgs args)
    {
        try
        {
            var receiveData = JsonUtility.FromJson<MatchResponseData>(Encoding.UTF8.GetString(args.Result));

            // エラーが返ってきたら
            if (receiveData.Status != "SUCCESS")
            {
                Debug.LogError("Match Request Error : " + Encoding.UTF8.GetString(args.Result));
                EventUtility.SafeInvokeAction(m_FailureMatchRequestCallBack);
                m_FailureMatchRequestCallBack = null;
                return;
            }

            if (receiveData.Message == "OPEN_SERVER")
            {
                // 他のクライアントを待ち受ける
                OpenTcpServer(SelfIpAddress, m_P2Pport, OnAcceptTcpClient);
                Debug.Log("Open Master Client as " + SelfIpAddress);
            }
            else if (receiveData.Message == "OPEN_CLIENT")
            {
                IPAddress ipAddress;
                if (!IPAddress.TryParse(receiveData.Addr, out ipAddress))
                {
                    Debug.LogError("Match Request Error : 対戦相手のIPv4アドレスのパースに失敗しました。IPv4 : " + receiveData.Addr);
                    EventUtility.SafeInvokeAction(m_FailureMatchRequestCallBack);
                    m_FailureMatchRequestCallBack = null;
                    return;
                }

                // 待ち受けているクライアントに接続する
                OpponentIpAddress = ipAddress;
                ConnectTcpServer(OpponentIpAddress, m_P2Pport, OnConnectTcpClient);
                Debug.Log("Connect Master Client to " + OpponentIpAddress);
            }
        }
        catch (SocketException se)
        {
            Debug.LogError("Match Request Error : エラーが発生しました。");
            Debug.LogException(se);
            EventUtility.SafeInvokeAction(m_FailureMatchRequestCallBack);
            m_FailureMatchRequestCallBack = null;

        }
    }

    /// <summary>
    /// Tcpクライアント接続要求コールバック。
    /// </summary>
    /// <param name="result">接続要求の結果データ</param>
    private void OnAcceptTcpClient(IAsyncResult result)
    {
        Debug.Log("Accept Other Client!");

        var listener = (TcpListener)result.AsyncState;
        var tcpClient = listener.EndAcceptTcpClient(result);
        listener.Stop();

        // 接続してきた相手のアドレスを取得する
        var ep = (IPEndPoint)tcpClient.Client.RemoteEndPoint;
        OpponentIpAddress = ep.Address;

        // クライアント間の接続開始
        CreateNetproUdpClient();
        CreateNetproTcpClient(tcpClient);
        EventUtility.SafeInvokeAction(m_MatchCallBack);
    }

    /// <summary>
    /// Tcpクライアント接続コールバック。
    /// </summary>
    /// <param name="result">接続の結果データ</param>
    private void OnConnectTcpClient(IAsyncResult result)
    {
        Debug.Log("Connect Master Client!");

        var tcpClient = (TcpClient)result.AsyncState;

        // クライアント間の接続開始
        CreateNetproUdpClient();
        CreateNetproTcpClient(tcpClient);
        EventUtility.SafeInvokeAction(m_MatchCallBack);
    }

    #endregion
}
