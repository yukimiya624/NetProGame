using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchRequestProcess : MonoBehaviour
{
    [SerializeField]
    private Dropdown m_Dropdown;

    [SerializeField]
    private Button m_RequestMatchButton;

    private void Start()
    {
        m_Dropdown.ClearOptions();
        var addresses = NetproNetworkManager.Instance.GetSelfIpAddresses();
        var list = new List<Dropdown.OptionData>();
        foreach (var addr in addresses)
        {
            var data = new Dropdown.OptionData();
            data.text = addr.ToString();
            list.Add(data);
        }

        m_Dropdown.AddOptions(list);

        m_RequestMatchButton.onClick.AddListener(OnClickRequestMatch);
    }

    private void OnClickRequestMatch()
    {
        var idx = m_Dropdown.value;
        var data = m_Dropdown.options[idx];
        GameManager.Instance.RequestMatch(data.text);
    }
}
