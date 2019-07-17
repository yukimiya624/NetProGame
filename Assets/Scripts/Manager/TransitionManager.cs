using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// シーン遷移時処理を管理する。
/// </summary>
public class TransitionManager : SingletonMonoBehavior<TransitionManager>
{
	[SerializeField]
	private TransitionController m_DefaultTransition;

	private bool m_IsHiding;

	private bool m_IsShowing;

    protected override void OnAwake()
    {
        base.OnAwake();
    }

    public void Hide( Action onComplete )
	{
		m_DefaultTransition.Hide( onComplete );
	}

	public void Show( Action onComplete )
	{
		m_DefaultTransition.Show( onComplete );
	}
}
