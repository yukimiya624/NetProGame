using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// ステートマシン。
/// Created by Sho Yamagami.
/// </summary>
public class StateMachine<T>
{
    private Dictionary<T, State<T>> m_States;

    private State<T> m_CurrentState;

    public StateMachine()
    {
        m_States = new Dictionary<T, State<T>>();
    }

    public void OnFinalize()
    {
        m_States.Clear();
        m_States = null;
    }

    public void OnUpdate()
    {
        if (m_CurrentState != null)
        {
            EventUtility.SafeInvokeAction(m_CurrentState.m_OnUpdate);
        }
    }

    public void OnLateUpdate()
    {
        if (m_CurrentState != null)
        {
            EventUtility.SafeInvokeAction(m_CurrentState.m_OnLateUpdate);
        }
    }

    public void OnFixedUpdate()
    {
        if (m_CurrentState != null)
        {
            EventUtility.SafeInvokeAction(m_CurrentState.m_OnFixedUpdate);
        }
    }

    public void AddState(State<T> state)
    {
        if (state == null)
        {
            return;
        }

        m_States.Add(state.m_Key, state);
    }

    public void Goto(T key)
    {
        if (m_States == null)
        {
            return;
        }

        if (!m_States.ContainsKey(key))
        {
            return;
        }

        var nextState = m_States[key];

        if (m_CurrentState != null)
        {
            EventUtility.SafeInvokeAction(m_CurrentState.m_OnEnd);
            m_CurrentState = null;
        }

        if (nextState != null)
        {
            EventUtility.SafeInvokeAction(nextState.m_OnStart);
            m_CurrentState = nextState;
        }
    }
}
