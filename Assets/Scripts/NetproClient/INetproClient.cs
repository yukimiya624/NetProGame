using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface INetproClient
{
    Action OnReceive { get; set; }

    void Start();

    void End();

    void SendData(string data, Action failedSendCallback);

    string GetReceivedData();

    bool IsRemainReceivedData();

    IEnumerable GetAllReceivedData();
}
