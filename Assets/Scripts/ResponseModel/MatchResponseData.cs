using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// マッチ待機レスポンスデータ
/// </summary>
[Serializable]
public struct MatchResponseData
{
    /// <summary>
    /// レスポンスステータス
    /// </summary>
    public string Status;

    /// <summary>
    /// レスポンスメッセージ
    /// </summary>
    public string Message;

    /// <summary>
    /// マスタークライアントのIPv4アドレス
    /// </summary>
    public string Addr;

    public string Memo;
}
