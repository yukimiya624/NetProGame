using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// タイマーの種類。
/// </summary>
public enum E_TIMER_TYPE
{
	/// <summary>
	/// このタイマーはTimeScaleに影響される。
	/// ゲーム内の秒間隔に対するタイマーとなる。
	/// </summary>
	SCALED_TIMER,

	/// <summary>
	/// このタイマーはTimeScaleに影響されない。
	/// 現実の秒間隔に対するタイマーとなる。
	/// </summary>
	UNSCALED_TIMER,
}