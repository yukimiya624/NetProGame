using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineController
{

    private LinkedList<IEnumerator> m_CoroutineList;

    private LinkedList<IEnumerator> m_GotoStopCoroutineList;

    public CoroutineController()
    {
        m_CoroutineList = new LinkedList<IEnumerator>();
        m_GotoStopCoroutineList = new LinkedList<IEnumerator>();
    }

    public void OnUpdate()
    {
        foreach (var coroutine in m_CoroutineList)
        {
            if (coroutine == null)
            {
                continue;
            }

            var existNext = coroutine.MoveNext();
            if (!existNext)
            {
                m_GotoStopCoroutineList.AddLast(coroutine);
                continue;
            }
        }

        RemoveStopTimers();
    }

    /// <summary>
    /// 停止したタイマーを削除する。
    /// </summary>
    public void RemoveStopTimers()
    {
        int count = m_GotoStopCoroutineList.Count;

        for (int i = 0; i < count; i++)
        {
            var coroutine = m_GotoStopCoroutineList.First.Value;
            m_CoroutineList.Remove(coroutine);
            m_GotoStopCoroutineList.RemoveFirst();
        }
    }

    /// <summary>
    /// コルーチンを登録する。
    /// </summary>
    public void RegistCoroutine(IEnumerator coroutine)
    {
        if (coroutine == null || m_CoroutineList.Contains(coroutine))
        {
            return;
        }

        m_CoroutineList.AddLast(coroutine);
    }

    /// <summary>
    /// コルーチンを削除する。
    /// </summary>
    public void RemoveCoroutine(IEnumerator coroutine)
    {
        if (coroutine == null || m_GotoStopCoroutineList.Contains(coroutine))
        {
            return;
        }

        m_GotoStopCoroutineList.AddLast(coroutine);
    }
}
