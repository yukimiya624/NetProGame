using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class TransitionController : MonoBehaviour
{
	protected bool m_IsHiding;

	protected bool m_IsShowing;

	public void Hide( Action onComplete )
	{
		if( m_IsHiding )
		{
			return;
		}

		m_IsHiding = true;
		StartCoroutine( OnHide( () =>
		{
			EventUtility.SafeInvokeAction( onComplete );
			m_IsHiding = false;
		} ) );
	}

	public void Show( Action onComplete )
	{
		if( m_IsShowing )
		{
			return;
		}

		m_IsShowing = true;
		StartCoroutine( OnShow( () =>
		{
			EventUtility.SafeInvokeAction( onComplete );
			m_IsShowing = false;
		} ) );
	}

	protected abstract IEnumerator OnHide( Action onComplete );

	protected abstract IEnumerator OnShow( Action onComplete );
}
