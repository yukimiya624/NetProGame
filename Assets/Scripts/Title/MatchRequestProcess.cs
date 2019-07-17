using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// マッチリクエストを行うための一連の処理をまとめたコンポーネント。
/// Create by Sho Yamagami.
/// </summary>
public class MatchRequestProcess : MonoBehaviour
{
#pragma warning disable 649

    [SerializeField]
    private Dropdown m_AddressDropdown;

    [SerializeField]
    private Button m_RequestMatchButton;

#pragma warning restore 649

    public Action<string> OnClickMatchRequest { get; set; }

    private void Start()
    {
        InitAddressDropDown();
        m_RequestMatchButton.onClick.AddListener(OnClickRequestMatch);
    }

    /// <summary>
    /// アドレスドロップダウンの初期化。
    /// </summary>
    private void InitAddressDropDown()
    {
        m_AddressDropdown.ClearOptions();
        var addresses = NetproNetworkManager.Instance.FindSelfIpAddresses();
        var list = new List<Dropdown.OptionData>();
        foreach (var addr in addresses)
        {
            var data = new Dropdown.OptionData();
            data.text = addr.ToString();
            list.Add(data);
        }
        m_AddressDropdown.AddOptions(list);
    }

    /// <summary>
    /// マッチリクエストボタンを押した時の処理。
    /// </summary>
    private void OnClickRequestMatch()
    {
        var idx = m_AddressDropdown.value;
        var data = m_AddressDropdown.options[idx];
        EventUtility.SafeInvokeAction(OnClickMatchRequest, data.text);
    }
}
