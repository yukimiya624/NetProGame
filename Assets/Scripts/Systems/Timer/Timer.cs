using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// タイマー。
/// </summary>
public class Timer
{
	#region Field

	/// <summary>
	/// このタイマーが登録されているTimeController
	/// </summary>
	private TimerController m_RegistingTimerController;

	/// <summary>
	/// このタイマーのスケールの種類。
	/// </summary>
	private E_TIMER_TYPE m_TimerType;

	/// <summary>
	/// このタイマーのサイクル。
	/// </summary>
	private E_TIMER_CYCLE m_TimerCycle;

	/// <summary>
	/// 毎フレーム呼び出されるコールバック。
	/// </summary>
	private Action m_StepCallBack;

	/// <summary>
	/// 指定した秒数毎に呼び出されるコールバック。
	/// </summary>
	private Action m_IntervalCallBack;

	/// <summary>
	/// タイマー終了時に呼び出されるコールバック。
	/// </summary>
	private Action m_TimeoutCallBack;

	/// <summary>
	/// タイマーが一時停止状態になった時に呼び出されるコールバック。
	/// </summary>
	private Action m_OnPauseCallBack;

	/// <summary>
	/// タイマーが一時停止状態から再開した時に呼び出されるコールバック。
	/// </summary>
	private Action m_OnResumeCallBack;

	/// <summary>
	/// タイマーが完全停止状態になった時に呼び出されるコールバック。
	/// </summary>
	private Action m_OnStopCallBack;

	/// <summary>
	/// カウントダウンまでの時間。
	/// </summary>
	private float m_TimeoutDuration;

	/// <summary>
	/// インターバル時間。
	/// </summary>
	private float m_IntervalDuration;

	/// <summary>
	/// カウントダウンを保持するカウンター。
	/// </summary>
	private float m_TimeoutCount;

	/// <summary>
	/// インターバルを保持するカウンター。
	/// </summary>
	private float m_IntervalCount;

	#endregion



	#region Getter & Setter

	public TimerController GetTimerController()
	{
		return m_RegistingTimerController;
	}

	public Timer SetTimerController( TimerController controller )
	{
		m_RegistingTimerController = controller;
		return this;
	}

	public E_TIMER_TYPE GetTimerType()
	{
		return m_TimerType;
	}

	public E_TIMER_CYCLE GetTimerCycle()
	{
		return m_TimerCycle;
	}

	public Timer SetTimerCycle( E_TIMER_CYCLE cycle )
	{
		m_TimerCycle = cycle;
		return this;
	}

	public Action GetStepCallBack()
	{
		return m_StepCallBack;
	}

	public Timer SetStepCallBack( Action callBack )
	{
		m_StepCallBack = callBack;
		return this;
	}

	public Action GetIntervalCallBack()
	{
		return m_IntervalCallBack;
	}

	public Timer SetIntervalCallBack( Action callBack )
	{
		m_IntervalCallBack = callBack;
		return this;
	}

	public Action GetTimeoutCallBack()
	{
		return m_TimeoutCallBack;
	}

	public Timer SetTimeoutCallBack( Action callBack )
	{
		m_TimeoutCallBack = callBack;
		return this;
	}

	public Action GetPauseCallBack()
	{
		return m_OnPauseCallBack;
	}

	public Timer SetPauseCallBack( Action callBack )
	{
		m_OnPauseCallBack = callBack;
		return this;
	}

	public Action GetResumeCallBack()
	{
		return m_OnResumeCallBack;
	}

	public Timer SetResumeCallBack( Action callBack )
	{
		m_OnResumeCallBack = callBack;
		return this;
	}

	public Action GetStopCallBack()
	{
		return m_OnStopCallBack;
	}

	public Timer SetStopCallBack( Action callBack )
	{
		m_OnStopCallBack = callBack;
		return this;
	}

	public float GetTimeoutDuration()
	{
		return m_TimeoutDuration;
	}

	public float GetIntervalDuration()
	{
		return m_IntervalDuration;
	}

	public float GetTimeoutCount()
	{
		return m_TimeoutCount;
	}

	public float GetIntervalCount()
	{
		return m_IntervalCount;
	}

	#endregion



	private Timer( E_TIMER_TYPE timerType )
	{
		m_TimeoutDuration = -1;
		m_IntervalDuration = -1;
		m_TimeoutCount = 0;
		m_IntervalCount = 0;
		m_TimerCycle = E_TIMER_CYCLE.STANDBY;
		m_TimerType = timerType;
	}

	/// <summary>
	/// タイマーを作成する。
	/// </summary>
	/// <param name="timerType">タイマータイプ</param>
	/// <param name="intervalDuration">インターバルタイム</param>
	/// <param name="timeoutDuration">カウントダウンタイム</param>
	/// <param name="stepCallBack">毎フレーム呼び出されるコールバック</param>
	/// <param name="intervalCallBack">インターバルコールバック</param>
	/// <param name="timeoutCallBack">カウントが0になった時のコールバック</param>
	/// <param name="onPauseCallBack">一時停止時コールバック</param>
	/// <param name="onResumeCallBack">再開時コールバック</param>
	/// <param name="onStopCallBack">タイマーストップコールバック</param>
	public static Timer CreateTimer( E_TIMER_TYPE timerType, float intervalDuration, float timeoutDuration, Action stepCallBack = null, Action intervalCallBack = null, Action timeoutCallBack = null, Action onPauseCallBack = null, Action onResumeCallBack = null, Action onStopCallBack = null )
	{
		var timer = new Timer( timerType );
		timer.m_IntervalDuration = intervalDuration;
		timer.m_TimeoutDuration = timeoutDuration;
		timer.m_StepCallBack = stepCallBack;
		timer.m_IntervalCallBack = intervalCallBack;
		timer.m_TimeoutCallBack = timeoutCallBack;
		timer.m_OnPauseCallBack = onPauseCallBack;
		timer.m_OnResumeCallBack = onResumeCallBack;
		timer.m_OnStopCallBack = onStopCallBack;

		return timer;
	}

    public static Timer CreateTimeoutTimer(E_TIMER_TYPE timerType, float timeoutDuration)
    {
        var timer = CreateTimeoutTimer(timerType, timeoutDuration, null, null);
        return timer;
    }

	/// <summary>
	/// カウントダウンタイマーを作成する。
	/// </summary>
	/// <param name="timerType">タイマータイプ</param>
	/// <param name="timeoutDuration">カウントダウンタイム</param>
	/// <param name="stepCallBack">毎フレーム呼び出されるコールバック</param>
	/// <param name="timeoutCallBack">カウントが0になった時のコールバック</param>
	public static Timer CreateTimeoutTimer( E_TIMER_TYPE timerType, float timeoutDuration, Action stepCallBack, Action timeoutCallBack )
	{
		var timer = CreateTimer( timerType, -1, timeoutDuration, stepCallBack, null, timeoutCallBack );
		return timer;
	}

	/// <summary>
	/// カウントダウンタイマーを作成する。
	/// </summary>
	/// <param name="timerType">タイマータイプ</param>
	/// <param name="timeoutDuration">カウントダウンタイム</param>
	/// <param name="timeoutCallBack">カウントが0になった時のコールバック</param>
	public static Timer CreateTimeoutTimer( E_TIMER_TYPE timerType, float timeoutDuration, Action timeoutCallBack )
	{
		var timer = CreateTimeoutTimer( timerType, timeoutDuration, null, timeoutCallBack );
		return timer;
	}

	/// <summary>
	/// インターバルタイマーを作成する。
	/// </summary>
	/// <param name="timerType">タイマータイプ</param>
	/// <param name="intervalDuration">インターバルタイム</param>
	/// <param name="stepCallBack">毎フレーム呼び出されるコールバック</param>
	/// <param name="intervalCallBack">インターバルコールバック</param>
	public static Timer CreateIntervalTimer( E_TIMER_TYPE timerType, float intervalDuration, Action stepCallBack, Action intervalCallBack )
	{
		var timer = CreateTimer( timerType, intervalDuration, -1, stepCallBack, intervalCallBack );
		return timer;
	}

	/// <summary>
	/// インターバルタイマーを作成する。
	/// </summary>
	/// <param name="timerType">タイマータイプ</param>
	/// <param name="intervalDuration">インターバルタイム</param>
	/// <param name="intervalCallBack">インターバルコールバック</param>
	public static Timer CreateIntervalTimer( E_TIMER_TYPE timerType, float intervalDuration, Action intervalCallBack )
	{
		var timer = CreateIntervalTimer( timerType, intervalDuration, null, intervalCallBack );
		return timer;
	}



	public void OnFixedUpdate()
	{
		if( m_TimerCycle != E_TIMER_CYCLE.UPDATE )
		{
			return;
		}

		if( m_TimerType == E_TIMER_TYPE.SCALED_TIMER )
		{
			if( m_TimeoutDuration > 0 )
			{
				m_TimeoutCount += Time.fixedDeltaTime;
			}

			if( m_IntervalDuration > 0 )
			{
				m_IntervalCount += Time.fixedDeltaTime;
			}
		}
		else
		{
			if( m_TimeoutDuration > 0 )
			{
				m_TimeoutCount += Time.fixedUnscaledDeltaTime;
			}

			if( m_IntervalDuration > 0 )
			{
				m_IntervalCount += Time.fixedUnscaledDeltaTime;
			}
		}

		// ステップコールバック
		EventUtility.SafeInvokeAction( m_StepCallBack );

		// インターバルコールバック
		if( m_IntervalDuration > 0 && m_IntervalCount >= m_IntervalDuration )
		{
			EventUtility.SafeInvokeAction( m_IntervalCallBack );
			m_IntervalCount = 0;
		}

		// タイムアウトコールバック
		if( m_TimeoutDuration > 0 && m_TimeoutCount >= m_TimeoutDuration )
		{
			m_TimerCycle = E_TIMER_CYCLE.STOP;
			EventUtility.SafeInvokeAction( m_TimeoutCallBack );

			if( m_RegistingTimerController != null )
			{
				m_RegistingTimerController.RemoveTimer( this );
			}
		}
	}



	/// <summary>
	/// タイマーを一時停止する。
	/// </summary>
	public void PauseTimer()
	{
		if( m_TimerCycle != E_TIMER_CYCLE.UPDATE )
		{
			return;
		}

		m_TimerCycle = E_TIMER_CYCLE.PAUSE;
		EventUtility.SafeInvokeAction( m_OnPauseCallBack );
	}

	/// <summary>
	/// タイマーを再開する。
	/// </summary>
	public void ResumeTimer()
	{
		if( m_TimerCycle != E_TIMER_CYCLE.PAUSE )
		{
			return;
		}

		m_TimerCycle = E_TIMER_CYCLE.UPDATE;
		EventUtility.SafeInvokeAction( m_OnResumeCallBack );
	}

	/// <summary>
	/// タイマーを完全停止する。
	/// </summary>
	public void StopTimer()
	{
		if( m_TimerCycle != E_TIMER_CYCLE.UPDATE && m_TimerCycle != E_TIMER_CYCLE.PAUSE )
		{
			return;
		}

		m_TimerCycle = E_TIMER_CYCLE.STOP;
		EventUtility.SafeInvokeAction( m_OnStopCallBack );

		if( m_RegistingTimerController != null )
		{
			m_RegistingTimerController.RemoveTimer( this );
		}
	}

	/// <summary>
	/// タイマーを完全停止させて破棄する。
	/// </summary>
	public void DestroyTimer()
	{
		m_RegistingTimerController.RemoveTimer( this );
	}
}
