using System;
using System.Net;
using System.Text;

public class NetproApiClient : WebClient
{
    public static string HTTP_REQUEST_CONTENT_TYPE = "application/json";
    public static string HTTP_REQUEST_ACCEPT = "application/json";

    private UploadDataCompletedEventHandler m_UploadDataCompleteEventHandler;
    private Action<UploadDataCompletedEventArgs> m_OnUploadDataComplete;

    public NetproApiClient() : base()
    {
        m_UploadDataCompleteEventHandler = new UploadDataCompletedEventHandler(OnUploadDataCompleted);
    }

    public bool UploadDataAsync(string url, int timeout, string uploadData, Action<UploadDataCompletedEventArgs> callBack)
    {
        if (m_OnUploadDataComplete != null)
        {
            return false;
        }

        Headers[HttpRequestHeader.ContentType] = HTTP_REQUEST_CONTENT_TYPE;
        Headers[HttpRequestHeader.Accept] = HTTP_REQUEST_ACCEPT;
        Encoding = Encoding.UTF8;

        var uri = new Uri(url);
        //var webRequest = GetWebRequest(uri);
        //webRequest.Timeout = timeout;

        m_OnUploadDataComplete = callBack;
        UploadDataCompleted += m_UploadDataCompleteEventHandler;
        UploadDataAsync(uri, Encoding.UTF8.GetBytes(uploadData));

        return true;
    }

    private void OnUploadDataCompleted(object sender, UploadDataCompletedEventArgs args)
    {
        if (m_OnUploadDataComplete != null)
        {
            m_OnUploadDataComplete.Invoke(args);
            m_OnUploadDataComplete = null;
        }
    }
}
