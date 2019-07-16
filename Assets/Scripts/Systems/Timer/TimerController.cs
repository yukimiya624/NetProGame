using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerController
{

	private LinkedList<Timer> m_TimerList;

	private LinkedList<Timer> m_GotoStopTimerList;

	/// <summary>
	/// TimerManagerのタイマーサイクル。
	/// </summary>
	private E_TIMER_CYCLE m_TimerCycle;

	public TimerController()
	{
		m_TimerList = new LinkedList<Timer>();
		m_GotoStopTimerList = new LinkedList<Timer>();
		m_TimerCycle = E_TIMER_CYCLE.UPDATE;
	}

	/// <summary>
	/// 1秒間に FixedTimeStep * TimeScale 回呼び出される。
	/// </summary>
	public void OnFixedUpdate()
	{
		if( m_TimerCycle != E_TIMER_CYCLE.UPDATE )
		{
			return;
		}

		foreach( var timer in m_TimerList )
		{
			if( timer != null )
			{
				timer.OnFixedUpdate();
			}
		}

		RemoveStopTimers();
	}

	/// <summary>
	/// 停止したタイマーを削除する。
	/// </summary>
	public void RemoveStopTimers()
	{
		int count = m_GotoStopTimerList.Count;

		for( int i = 0; i < count; i++ )
		{
			var timer = m_GotoStopTimerList.First.Value;
			m_TimerList.Remove( timer );
			m_GotoStopTimerList.RemoveFirst();
		}
	}

	/// <summary>
	/// タイマーを登録する。
	/// </summary>
	public void RegistTimer( Timer timer )
	{
		if( timer == null )
		{
			return;
		}

		m_TimerList.AddLast( timer );
		timer.SetTimerCycle( E_TIMER_CYCLE.UPDATE );
		timer.SetTimerController( this );
	}

	/// <summary>
	/// タイマーを削除する。
	/// </summary>
	public void RemoveTimer( Timer timer )
	{
		if( timer == null )
		{
			return;
		}

		if( timer.GetTimerCycle() != E_TIMER_CYCLE.UPDATE && timer.GetTimerCycle() != E_TIMER_CYCLE.PAUSE )
		{
			timer.SetTimerCycle( E_TIMER_CYCLE.STOP );
			EventUtility.SafeInvokeAction( timer.GetStopCallBack() );
		}

		m_GotoStopTimerList.AddLast( timer );
	}

	/// <summary>
	/// TimerManager全体で一時停止する。
	/// </summary>
	public void PauseTimerManager()
	{
		if( m_TimerCycle != E_TIMER_CYCLE.UPDATE )
		{
			return;
		}

		m_TimerCycle = E_TIMER_CYCLE.PAUSE;
	}

	/// <summary>
	/// TimerManager全体で一時停止を解除する。
	/// </summary>
	public void ResumeTimerManager()
	{
		if( m_TimerCycle != E_TIMER_CYCLE.PAUSE )
		{
			return;
		}

		m_TimerCycle = E_TIMER_CYCLE.UPDATE;
	}
}
