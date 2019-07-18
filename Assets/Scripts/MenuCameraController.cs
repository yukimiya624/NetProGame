using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// タイトルやリザルト等の汎用カメラコントローラ。
/// Created by Sho Yamagami.
/// </summary>
public class MenuCameraController : MonoBehaviour
{
    #region Field Inspector
#pragma warning disable 649

    /// <summary>
    /// 基準点
    /// </summary>
    [SerializeField]
    private Vector3 m_BasePosition;

    /// <summary>
    /// 仰角
    /// </summary>
    [SerializeField]
    private float m_ElevationAngle;

    /// <summary>
    /// 基準点からの距離
    /// </summary>
    [SerializeField]
    private float m_Distance;

    /// <summary>
    /// カメラを振る時の基準角度
    /// </summary>
    [SerializeField]
    private float m_BaseAngle;

    /// <summary>
    /// カメラを振る時の振幅
    /// </summary>
    [SerializeField]
    private float m_AngleAmplitude;

    /// <summary>
    /// カメラを振る時の角速度
    /// </summary>
    [SerializeField]
    private float m_RadSpeed;

#pragma warning restore 649
    #endregion



    #region Field 

    private Camera m_Camera;
    private float m_Rad;
    private float m_Angle;

    #endregion



    private void Awake()
    {
        m_Camera = GetComponent<Camera>();
        m_Angle = m_BaseAngle * Mathf.Deg2Rad;
    }

    private void LateUpdate()
    {
        var eAngle = m_ElevationAngle * Mathf.Deg2Rad;
        var x = m_Distance * Mathf.Cos(m_Angle) * Mathf.Cos(eAngle);
        var y = m_Distance * Mathf.Sin(eAngle);
        var z = m_Distance * Mathf.Sin(m_Angle) * Mathf.Cos(eAngle);

        var pos = new Vector3(x, y, z) + m_BasePosition;
        m_Camera.transform.position = pos;
        m_Camera.transform.LookAt(m_BasePosition);

        // 角度更新
        m_Rad += m_RadSpeed * Time.deltaTime;
        m_Rad %= Mathf.PI * 2;
        m_Angle = m_AngleAmplitude * Mathf.Sin(m_Rad) + m_BaseAngle * Mathf.Deg2Rad;
    }
}
