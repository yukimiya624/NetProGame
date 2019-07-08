using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// マッチ待機リクエストデータ
/// </summary>
[Serializable]
public struct MatchRequestData
{
    /// <summary>
    /// クライアントのIPv4アドレス
    /// </summary>
    public string Addr;

    public MatchRequestData(string addr)
    {
        Addr = addr;
    }
}
