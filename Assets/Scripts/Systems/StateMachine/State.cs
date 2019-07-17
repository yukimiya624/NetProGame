using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// ステートマシンで使用するステート。
/// Created by Sho Yamagami.
/// </summary>
public class State<T>
{
    public T m_Key { get; private set; }

    public Action m_OnStart;
    public Action m_OnUpdate;
    public Action m_OnLateUpdate;
    public Action m_OnFixedUpdate;
    public Action m_OnEnd;

    public State(T key)
    {
        m_Key = key;
    }

    ~State()
    {
        m_OnStart = null;
        m_OnUpdate = null;
        m_OnLateUpdate = null;
        m_OnFixedUpdate = null;
        m_OnEnd = null;
    }
}
