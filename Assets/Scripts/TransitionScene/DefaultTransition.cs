using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DefaultTransition : TransitionController
{
	[SerializeField]
	private Image m_BackGround;

	[SerializeField]
	private Text m_LoadingText;

	[SerializeField]
	private float m_Duration;

	protected override IEnumerator OnHide( Action onComplete )
	{
		var bgColor = Color.black;

		m_BackGround.DOColor( bgColor, m_Duration );

		var textColor = Color.white;

		m_LoadingText.DOColor( textColor, m_Duration );

		yield return new WaitForSeconds( m_Duration );

		EventUtility.SafeInvokeAction( onComplete );
	}

	protected override IEnumerator OnShow( Action onComplete )
	{
		var bgColor = Color.black;
		bgColor.a = 0;

		m_BackGround.DOColor( bgColor, m_Duration );

		var textColor = Color.white;
		textColor.a = 0;

		m_LoadingText.DOColor( textColor, m_Duration );

		yield return new WaitForSeconds( m_Duration );

		EventUtility.SafeInvokeAction( onComplete );
	}
}
