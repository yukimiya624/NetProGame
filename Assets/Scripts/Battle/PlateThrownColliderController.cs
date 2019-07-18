using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlateThrownColliderController : MonoBehaviour
{
    [SerializeField]
    private Collider m_Collider;

    private Action m_GroundTouchCallback;

    public void SetGroundTouchCallback(Action callback)
    {
        m_GroundTouchCallback = callback;
    }

    private void OnTriggerEnter(Collider other)
    {
        // グラウンドに接地したら
        switch (other.gameObject.tag)
        {
            case TagName.Ground:
                EventUtility.SafeInvokeAction(m_GroundTouchCallback);
                break;
        }
    }
}
