using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// タイマーのサイクル。
/// </summary>
public enum E_TIMER_CYCLE
{
	/// <summary>
	/// タイマーが始まる前の状態。
	/// </summary>
	STANDBY,

	/// <summary>
	/// タイマーが動いている状態。
	/// </summary>
	UPDATE,

	/// <summary>
	/// タイマーが一時停止している状態。
	/// </summary>
	PAUSE,

	/// <summary>
	/// タイマーが停止している状態。
	/// </summary>
	STOP,
}