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
    private State<T> m_PreState;
    private State<T> m_NextState;

    public StateMachine()
    {
        m_States = new Dictionary<T, State<T>>();
        m_CurrentState = null;
        m_PreState = null;
        m_NextState = null;
    }

    public void OnFinalize()
    {
        m_States.Clear();
        m_States = null;
    }

    public void OnUpdate()
    {
        if (m_CurrentState != m_NextState)
        {
            ProcessChangeState();
        }

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

    public State<T> GetCurrentState()
    {
        return m_CurrentState;
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
        Debug.Log(key);
        if (m_States == null)
        {
            return;
        }

        if (!m_States.ContainsKey(key))
        {
            return;
        }

        m_NextState = m_States[key];
    }

    private void ProcessChangeState()
    {
        if (m_CurrentState != null)
        {
            m_PreState = m_CurrentState;
            EventUtility.SafeInvokeAction(m_PreState.m_OnEnd);
        }

        if (m_NextState != null)
        {
            m_CurrentState = m_NextState;
            EventUtility.SafeInvokeAction(m_CurrentState.m_OnStart);
        }
    }
}
