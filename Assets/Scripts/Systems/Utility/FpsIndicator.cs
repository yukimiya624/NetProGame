using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fpsを表示する。
/// </summary>
public class FpsIndicator : MonoBehaviour
{
    private enum IndicatorType
    {
        CONSOLE,
        UI_TEXT,
    }

    [SerializeField]
    private IndicatorType m_IndicatorType;

    [SerializeField]
    private float m_UpdateInterval;

    [SerializeField]
    private Text m_OutText;

    private int m_FrameCount;
    private float m_PrevTime;

    private void Start()
    {
        m_FrameCount = 0;
        m_PrevTime = 0.0f;
    }

    private void Update()
    {
        m_FrameCount++;
        float time = Time.realtimeSinceStartup - m_PrevTime;

        if (time >= m_UpdateInterval)
        {
            var fps = string.Format("fps : {0}", (m_FrameCount / time).ToString("f2"));
            switch(m_IndicatorType)
            {
                case IndicatorType.CONSOLE:
                    Debug.Log(fps);
                    break;
                case IndicatorType.UI_TEXT:
                    m_OutText.text = fps;
                    break;
            }

            m_FrameCount = 0;
            m_PrevTime = Time.realtimeSinceStartup;
        }
    }
}
