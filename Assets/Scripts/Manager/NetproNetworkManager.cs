using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// クライアント通信の低レイヤーをサポートするマネージャ。
/// Created by Sho Yamagami.
/// </summary>
public class NetproNetworkManager : SingletonMonoBehavior<NetproNetworkManager>
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

    [SerializeField]
    private int m_MasterUdpPort;

    [SerializeField]
    private int m_NonMasterUdpPort;

#pragma warning restore 649
    #endregion



    #region Field

    /// <summary>
    /// 対戦相手とマッチした時のコールバック。
    /// </summary>
    private Action m_MatchCallBack;

    /// <summary>
    /// マッチ待機開始時のコールバック。
    /// </summary>
    private Action m_MatchWaitCallBack;

    /// <summary>
    /// マッチリクエストで何らかの失敗が発生した時のコールバック。
    /// </summary>
    private Action m_FailureMatchRequestCallBack;

    #endregion



    #region Property

    /// <summary>
    /// 自分がマスタークライアントかどうか。
    /// </summary>
    public bool IsMasterClient { get; private set; }

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

    protected override void OnAwake()
    {
        base.OnAwake();
    }

    /// <summary>
    /// このインスタンスの破棄時の処理。
    /// </summary>
    protected override void OnDestroyed()
    {
        CloseClients();
        base.OnDestroyed();
    }

    /// <summary>
    /// 初期化処理。
    /// </summary>
    public override void OnInitialize()
    {
        base.OnInitialize();
    }

    /// <summary>
    /// 終了処理。
    /// </summary>
    public override void OnFinalize()
    {
        base.OnFinalize();
    }

    /// <summary>
    /// このインスタンスが生成されてから最初のフレームで呼び出される。
    /// </summary>
    public override void OnStart()
    {
        base.OnStart();
    }

    /// <summary>
    /// 毎フレーム呼び出される。
    /// </summary>
    public override void OnUpdate()
    {
        base.OnUpdate();
    }

    /// <summary>
    /// 全てのUpdate()関数が呼び出された後に毎フレーム呼び出される。
    /// </summary>
    public override void OnLateUpdate()
    {
        base.OnLateUpdate();
    }

    /// <summary>
    /// 固定間隔で呼び出される。
    /// </summary>
    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
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



    #region Connect

    /// <summary>
    /// 自身のPCのIPv4アドレスを全て取得する。
    /// </summary>
    /// <returns>IPv4アドレスのリスト</returns>
    public List<IPAddress> FindSelfIpAddresses()
    {
        IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());
        var list = new List<IPAddress>();

        foreach (var address in addresses)
        {
            // IPv4のみ取得する
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
    /// <param name="port">ポート</param>
    /// <param name="asyncCallback">接続完了時コールバック</param>
    public void AcceptTcpClient(IPAddress address, int port, Action<IAsyncResult> asyncCallback)
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
    public void ConnectTcpClient(IPAddress address, int port, Action<IAsyncResult> asyncCallback)
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
    private void CreateNetproUdpClient(bool isMasterClient)
    {
        if (OpponentIpAddress == null)
        {
            Debug.LogError("通信相手のIPv4アドレスがありません。");
            return;
        }

        if (!SelfIpAddress.Equals(OpponentIpAddress))
        {
            // アドレスが違う場合は、同じポートを使っても大丈夫
            UdpClient = new NetproUdpClient(OpponentIpAddress, m_MasterUdpPort, m_MasterUdpPort);
        }
        else
        {
            // アドレスが同じ場合(つまり同じ端末の場合)は、異なるポートを使ってUDP通信する
            if (isMasterClient)
            {
                UdpClient = new NetproUdpClient(OpponentIpAddress, m_MasterUdpPort, m_NonMasterUdpPort);
            }
            else
            {
                UdpClient = new NetproUdpClient(OpponentIpAddress, m_NonMasterUdpPort, m_MasterUdpPort);
            }
        }

        UdpClient.StartReceive();
    }

    /// <summary>
    /// UDPやTCPの通信で受信に失敗していないかどうかを判定する。
    /// </summary>
    public bool IsReceiveFailed(E_PROTOCOL_TYPE flag)
    {
        bool isFailed = false;
        if ((flag & E_PROTOCOL_TYPE.TCP) == E_PROTOCOL_TYPE.TCP && TcpClient != null)
        {
            isFailed |= TcpClient.IsReceiveFailed;
        }
        if ((flag & E_PROTOCOL_TYPE.UDP) == E_PROTOCOL_TYPE.UDP && UdpClient != null)
        {
            isFailed |= UdpClient.IsReceiveFailed;
        }

        return isFailed;
    }

    /// <summary>
    /// クライアントインスタンスを閉じる。
    /// </summary>
    public void CloseClients()
    {
        if (TcpClient != null)
        {
            try
            {
                TcpClient.EndClient();
            }
            finally
            {
                TcpClient = null;
            }
        }

        if (UdpClient != null)
        {
            try
            {
                UdpClient.EndClient();
            }
            finally
            {
                UdpClient = null;
            }
        }
    }

    #endregion



    #region Send Receive

    /// <summary>
    /// 構造体データをJSONに変換する
    /// </summary>
    private string ToJson<T>(T data) where T : struct
    {
        var typeData = new TypeData();
        typeData.Type = data.GetType().FullName;
        typeData.Content = JsonUtility.ToJson(data);
        return JsonUtility.ToJson(typeData);
    }

    /// <summary>
    /// JSONから構造体インスタンスに変換する
    /// </summary>
    /// <param name="data">タイプデータJSON</param>
    private object FromJson(string data)
    {
        var typeData = JsonUtility.FromJson<TypeData>(data);
        Type t = Type.GetType(typeData.Type);
        return JsonUtility.FromJson(typeData.Content, t);
    }

    /// <summary>
    /// TCPで構造体データを送信する
    /// </summary>
    /// <param name="data">送信する構造体</param>
    /// <param name="onFailedCallback">失敗時コールバック</param>
    public void SendTcp<T>(T data, Action<Exception> onFailedCallback = null) where T : struct
    {
        if (TcpClient == null)
        {
            Debug.LogError("Send TCP Error : TcpClientがnullです。");
            EventUtility.SafeInvokeAction(onFailedCallback, null);
            return;
        }

        var sendData = ToJson(data);
        TcpClient.SendData(sendData, onFailedCallback);
    }

    /// <summary>
    /// TCPで受信したデータを取得する
    /// </summary>
    public object ReceiveTcp()
    {
        if (TcpClient == null)
        {
            Debug.LogError("Receive TCP Error : TcpClientがnullです。");
            return null;
        }

        if (!TcpClient.IsRemainReceivedData())
        {
            return null;
        }

        var receivedData = TcpClient.GetReceivedData();
        return FromJson(receivedData);
    }

    /// <summary>
    /// UDPで構造体データを送信する
    /// </summary>
    /// <param name="data">送信する構造体</param>
    /// <param name="onFailedCallback">失敗時コールバック</param>
    public void SendUdp<T>(T data, Action<Exception> onFailedCallback = null) where T : struct
    {
        if (UdpClient == null)
        {
            Debug.LogError("Send UDP Error : UdpClientがnullです。");
            EventUtility.SafeInvokeAction(onFailedCallback, null);
            return;
        }

        var sendData = ToJson(data);
        UdpClient.SendData(sendData, onFailedCallback);
    }

    /// <summary>
    /// UDPで受信したデータを取得する
    /// </summary>
    public object ReceiveUdp()
    {
        if (UdpClient == null)
        {
            Debug.LogError("Receive UDP Error : UdpClientがnullです。");
            return null;
        }

        if (!UdpClient.IsRemainReceivedData())
        {
            return null;
        }

        var receivedData = UdpClient.GetReceivedData();
        return FromJson(receivedData);
    }

    #endregion



    #region Match

    private void ClearMatchProcessCallback()
    {
        m_MatchCallBack = null;
        m_MatchWaitCallBack = null;
        m_FailureMatchRequestCallBack = null;
    }

    /// <summary>
    /// マッチリクエストを送る。
    /// </summary>
    /// <param name="address">マッチに用いる自身のIPv4アドレス</param>
    /// <param name="seccessMatchCallback">マッチに成功した時のコールバック</param>
    /// <param name="matchWaitCallback">マッチ待機開始時のコールバック</param>
    /// <param name="failureRequestCallback">マッチに失敗した時のコールバック</param>
    public void RequestMatch(string address, Action seccessMatchCallback, Action matchWaitCallback, Action failureRequestCallback = null)
    {
        IPAddress addr;
        if (!IPAddress.TryParse(address, out addr))
        {
            Debug.LogError("Match Request Error : アドレスのパースに失敗しました。 address : " + address);
            EventUtility.SafeInvokeAction(failureRequestCallback);
            ClearMatchProcessCallback();
            return;
        }

        SelfIpAddress = addr;

        if (SelfIpAddress == null)
        {
            Debug.LogError("Match Request Error : 自身のIPアドレスを取得できませんでした");
            EventUtility.SafeInvokeAction(failureRequestCallback);
            ClearMatchProcessCallback();
            return;
        }

        var matchRequestData = new MatchRequestData(SelfIpAddress.ToString());
        var sendData = JsonUtility.ToJson(matchRequestData);
        var apiClient = new NetproApiClient();

        try
        {
            m_MatchCallBack = seccessMatchCallback;
            m_MatchWaitCallBack = matchWaitCallback;
            m_FailureMatchRequestCallBack = failureRequestCallback;
            apiClient.UploadDataAsync(m_RemoteServerUrl, 5000, sendData, OnRequestMatch);
        }
        catch (ArgumentException ae)
        {
            Debug.LogError("Match Request Error :  アドレスが不正です。");
            Debug.LogException(ae);
            EventUtility.SafeInvokeAction(m_FailureMatchRequestCallBack);
            ClearMatchProcessCallback();
        }
        catch (WebException we)
        {
            Debug.Log("Match Request Error : エラーが発生しました。");
            Debug.LogException(we);
            EventUtility.SafeInvokeAction(m_FailureMatchRequestCallBack);
            ClearMatchProcessCallback();
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
                ClearMatchProcessCallback();
                return;
            }

            if (receiveData.Message == "OPEN_SERVER")
            {
                // 他のクライアントを待ち受ける
                AcceptTcpClient(SelfIpAddress, m_P2Pport, OnAcceptTcpClient);
                Debug.Log("Open Master Client as " + SelfIpAddress);
                EventUtility.SafeInvokeAction(m_MatchWaitCallBack);
                m_MatchWaitCallBack = null;
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
                ConnectTcpClient(OpponentIpAddress, m_P2Pport, OnConnectTcpClient);
                Debug.Log("Connect Master Client to " + OpponentIpAddress);
                EventUtility.SafeInvokeAction(m_MatchWaitCallBack);
                m_MatchWaitCallBack = null;
            }
        }
        catch (SocketException se)
        {
            Debug.LogError("Match Request Error : エラーが発生しました。");
            Debug.LogException(se);
            EventUtility.SafeInvokeAction(m_FailureMatchRequestCallBack);
            ClearMatchProcessCallback();
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

        IsMasterClient = true;

        // クライアント間の接続開始
        CreateNetproTcpClient(tcpClient);
        CreateNetproUdpClient(IsMasterClient);
        EventUtility.SafeInvokeAction(m_MatchCallBack);
        ClearMatchProcessCallback();
    }

    /// <summary>
    /// Tcpクライアント接続コールバック。
    /// </summary>
    /// <param name="result">接続の結果データ</param>
    private void OnConnectTcpClient(IAsyncResult result)
    {
        Debug.Log("Connect Master Client!");

        var tcpClient = (TcpClient)result.AsyncState;

        IsMasterClient = false;

        // クライアント間の接続開始
        CreateNetproTcpClient(tcpClient);
        CreateNetproUdpClient(IsMasterClient);
        EventUtility.SafeInvokeAction(m_MatchCallBack);
        ClearMatchProcessCallback();
    }

    #endregion
}
