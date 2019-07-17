using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Dialog : MonoBehaviour
{
    [SerializeField]
    private Text m_Title;

    [SerializeField]
    private Text m_Body;

    [SerializeField]
    private Button m_Button;

    [SerializeField]
    private Text m_ButtonText;

    public void Show(string title, string body, string button, Action onClickButton)
    {

    }
}
