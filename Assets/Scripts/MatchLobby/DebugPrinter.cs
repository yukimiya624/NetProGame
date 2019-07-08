using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugPrinter : MonoBehaviour
{
    [SerializeField]
    private Text m_PrintText;

    [SerializeField]
    private VerticalLayoutGroup m_VerticalLayoutGroup;

    [SerializeField]
    private Button m_ClearButton;

    private void Awake()
    {
        Application.logMessageReceived += OnReceive;
        m_ClearButton.onClick.AddListener(ClearText);
        ClearText();
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnReceive;
    }

    private void ClearText()
    {
        if (m_PrintText != null)
        {
            m_PrintText.text = "";
        }
    }

    /// <summary>
    /// debugのテキストを受け取る
    /// </summary>
    private void OnReceive(string condition, string trace, LogType logType)
    {
        if (m_PrintText == null)
        {
            return;
        }

        var text = m_PrintText.text;

        switch (logType)
        {
            case LogType.Log:
                text += string.Format("{0}<color=#000000>{1}</color>{0}<color=#a0a0a0>{2}</color>{0}", System.Environment.NewLine, condition, trace);
                break;
            case LogType.Warning:
                text += string.Format("{0}<color=#adad00>{1}</color>{0}<color=#a0a000>{2}</color>{0}", System.Environment.NewLine, condition, trace);
                break;
            default:
                text += string.Format("{0}<color=#ad0000>{1}</color>{0}<color=#a00000>{2}</color>{0}", System.Environment.NewLine, condition, trace);
                break;
        }

        m_PrintText.text = text;
        m_VerticalLayoutGroup.CalculateLayoutInputVertical();
    }

    public void ResetLayout()
    {
        m_VerticalLayoutGroup.enabled = !m_VerticalLayoutGroup.enabled;
    }
}
