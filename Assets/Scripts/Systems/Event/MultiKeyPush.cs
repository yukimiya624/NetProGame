using UnityEngine;
using UnityEngine.Events;
using System;

public class MultiKeyPush : MonoBehaviour
{

	#region Definition

	[Serializable]
	public struct MultiKeySet
	{
		public KeyCode[] KeySet;

		public UnityEvent OnKeySetDown;

		public UnityEvent OnKeySetStay;

		public UnityEvent OnKeySetUp;

		public bool KeyDownFlag;
	}

	#endregion



	#region Field Private

	[SerializeField]
	private MultiKeySet[] m_KeySets;

	#endregion



	#region Method Unity Call

	private void Update()
	{
		for( int i = 0; i < m_KeySets.Length; i++ )
		{
			CheckKeySetEvent( ref m_KeySets[i] );
		}
	}

	#endregion



	#region Method Private

	private void CheckKeySetEvent( ref MultiKeySet keySet )
	{
		if( IsKeySetStay( keySet.KeySet ) )
		{
			if( !keySet.KeyDownFlag )
			{
				//　キーセットの押下が有効になった瞬間
				keySet.KeyDownFlag = true;
				EventUtility.SafeInvokeUnityEvent( keySet.OnKeySetDown );
			}
			else
			{
				// キーセットの有効性が続いている間
				EventUtility.SafeInvokeUnityEvent( keySet.OnKeySetStay );
			}
		}
		else
		{
			if( keySet.KeyDownFlag )
			{
				// キーセットの押下が無効になった瞬間
				keySet.KeyDownFlag = false;
				EventUtility.SafeInvokeUnityEvent( keySet.OnKeySetUp );
			}
		}
	}

	/// <summary>
	/// 全ての指定されたキーが押下中である場合、trueを返す。
	/// </summary>
	private bool IsKeySetStay( params KeyCode[] keySet )
	{
		foreach( var key in keySet )
		{
			if( !Input.GetKey( key ) )
				return false;
		}

		return true;
	}

	#endregion

}