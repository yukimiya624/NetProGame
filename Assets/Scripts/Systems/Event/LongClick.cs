using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// 長押しを検出するハンドラコンポーネントです。
/// 元URL : http://fantom1x.blog130.fc2.com/blog-entry-251.html
/// </summary>
public class LongClick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
	[SerializeField]
	private float m_ValidTime = 1f;

	public float ValidTime
	{
		get
		{
			return m_ValidTime;
		}
		set
		{
			m_ValidTime = value;
		}
	}

	[SerializeField]
	private UnityEvent m_OnLongClick = new UnityEvent();

	public UnityEvent OnLongClick
	{
		get
		{
			return m_OnLongClick;
		}
	}

	private float m_RequiredTime;
	private bool m_IsPressing = false;

	private void Update()
	{
		if( !m_IsPressing )
			return;

		if( Time.time >= m_RequiredTime )
		{
			EventUtility.SafeInvokeUnityEvent( m_OnLongClick );
			m_IsPressing = false;
		}
	}

	public void OnPointerDown( PointerEventData e )
	{
		if( !m_IsPressing )
		{
			m_IsPressing = true;
			m_RequiredTime = Time.time + m_ValidTime;
		}
		else
		{
			m_IsPressing = false;
		}
	}

	public void OnPointerUp( PointerEventData e )
	{
		m_IsPressing = false;
	}

	public void OnPointerExit( PointerEventData e )
	{
		m_IsPressing = false;
	}
}