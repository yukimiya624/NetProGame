using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// スワイプを検出するハンドラコンポーネントです。
/// 元URL : http://fantom1x.blog130.fc2.com/blog-entry-250.html
/// </summary>
public class Swipe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

	#region Definition

	[Serializable]
	public class SwipeHandler : UnityEvent<Vector2> { }

	public enum SwipeMoveKind
	{
		/// <summary>
		/// スワイプで引っ張られるのが四角になる。
		/// </summary>
		Cartesian,

		/// <summary>
		/// スワイプで引っ張られるのが円形になる。
		/// </summary>
		Polar,
	}

	#endregion



	#region Field Inspector

	[Header( "Target Transform" )]

	/// <summary>
	/// スワイプする時に引っ張られるトランスフォーム。
	/// </summary>
	[SerializeField]
	private Transform m_SwipeTransform;

	[Space()]
	[Header( "Swipe Handle Parameter" )]

	/// <summary>
	/// 画面幅のサイズの比率の基準。
	/// true : Screen.width, false : Screen.height
	/// </summary>
	[SerializeField]
	private bool m_WidthReference = true;

	/// <summary>
	/// スワイプとして認識する移動量の画面比率。
	/// この値より長い移動量をスワイプとして認識する。
	/// </summary>
	[SerializeField, Range( 0.0f, 1.0f )]
	private float m_ValidWidth = 0.25f;

	/// <summary>
	/// スワイプとして認識する時間。
	/// これより短い時間での操作をスワイプとして認識する。
	/// </summary>
	[SerializeField]
	private float m_Timeout = 0.5f;

	[Space()]
	[Header( "Swipe Move Parameter" )]

	/// <summary>
	/// スワイプで引っ張られる時の移動の種類。
	/// </summary>
	[SerializeField]
	private SwipeMoveKind m_SwipeMoveKind;

	/// <summary>
	/// スワイプで移動する限界量。
	/// 0 以上の実数で指定する。
	/// </summary>
	[SerializeField]
	private float m_SwipeMoveLimitDistance;

	/// <summary>
	/// スワイプで移動する時の減少率。
	/// 0 以上の実数で指定する。
	/// 0 の時動かなくなり、極大で限界量に追従する。
	/// </summary>
	[SerializeField]
	private float m_SwipeMoveReductionRate;

	/// <summary>
	/// スワイプ終了時に初期座標に戻る時間。
	/// </summary>
	[SerializeField]
	private float m_SwipeBackDuration;

	/// <summary>
	/// スワイプで移動する座標を制約する。
	/// true : 制約する false : 制約しない
	/// </summary>
	[SerializeField]
	private bool m_ConstrainX = false, m_ConstrainY = false;

	[Space()]
	[Header( "Swipe States" )]

	/// <summary>
	/// 押下されているかどうか。
	/// </summary>
	[SerializeField]
	private bool m_IsPressing;

	/// <summary>
	/// スワイプが始まっているかどうか。
	/// </summary>
	[SerializeField]
	private bool m_IsSwiping;

	/// <summary>
	/// 初期座標に戻っているかどうか。
	/// </summary>
	[SerializeField]
	private bool m_IsMoveToStart;

	[Space()]
	[Header( "Swipe Callback" )]

	/// <summary>
	/// スワイプを認識した時のコールバック。
	/// </summary>
	[SerializeField]
	private SwipeHandler m_OnSwipe = new SwipeHandler();

	#endregion



	#region Field Private

	/// <summary>
	/// マウス座標用。
	/// </summary>
	private Vector2 m_StartPos;

	/// <summary>
	/// スワイプ時のオブジェクト座標用。
	/// スワイプ初期のオブジェクト座標を保持する。
	/// </summary>
	private Vector3 m_SwipeStartPos;

	/// <summary>
	/// スワイプ時のオブジェクト座標用。
	/// マウス座標とスワイプ初期座標の差分を保持する。
	/// </summary>
	private Vector3 m_SwipeDeltaPos;

	private float m_LimitTime;

	private Vector2 m_SwipeDir = Vector2.zero;

	#endregion



	#region Property Public

	public bool IsSwiping
	{
		get
		{
			return m_IsSwiping;
		}
	}

	public SwipeHandler OnSwipe
	{
		get
		{
			return m_OnSwipe;
		}
	}

	#endregion



	#region Method Unity Call

	private void OnEnable()
	{
		m_IsPressing = false;
	}

	private void OnValidate()
	{
		if( m_SwipeMoveLimitDistance < 0 )
		{
			m_SwipeMoveLimitDistance = 0;
		}

		if( m_SwipeMoveReductionRate < 0 )
		{
			m_SwipeMoveReductionRate = 0;
		}
	}

	#endregion



	#region Method Implements

	public void OnBeginDrag( PointerEventData e )
	{
		if( Input.touchCount > 1 )
			return;

		if( m_IsPressing || m_IsMoveToStart )
			return;

		m_IsPressing = true;

		// スワイプ判定用の座標とスワイプで少し動かす用の座標は別にする
		m_StartPos = Input.mousePosition;

		// スワイプ初期の座標設定
		var mPos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
		m_SwipeStartPos = m_SwipeTransform.position;
		m_SwipeDeltaPos = mPos - m_SwipeStartPos;

		// スワイプ判定時間の設定
		m_LimitTime = Time.time + m_Timeout;
	}

	public void OnDrag( PointerEventData e )
	{
		if( Input.touchCount > 1 )
			return;

		if( !m_IsPressing || m_IsMoveToStart )
			return;

		m_IsSwiping = true;
		var mPos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
		var target = mPos - m_SwipeDeltaPos;
		MoveTo( m_SwipeStartPos, target );
	}

	public void OnEndDrag( PointerEventData e )
	{
		if( Input.touchCount > 1 )
			return;

		if( !m_IsPressing || m_IsMoveToStart )
			return;

		m_IsPressing = false;

		CallSwipeEvent();
		MoveToStartPosition();
	}

	#endregion



	#region Method Private

	/// <summary>
	/// オブジェクトの中心座標を指定した座標に変更する。
	/// </summary>
	private void MoveTo( Vector3 start, Vector3 target )
	{
		var delta = target - start;

		// 移動制約を掛ける
		if( m_ConstrainX )
		{
			delta.x = 0f;
		}

		if( m_ConstrainY )
		{
			delta.y = 0f;
		}

		delta.z = 0f;

		if( m_SwipeMoveLimitDistance < 0 )
		{
			m_SwipeMoveLimitDistance = 0;
		}

		delta.x = CalcSwipeMoveDistance( delta.x );
		delta.y = CalcSwipeMoveDistance( delta.y );

		if( m_SwipeMoveKind == SwipeMoveKind.Polar )
		{
			// xyを用いて円の半径を求める
			var theta = Mathf.Atan2( delta.y, delta.x );
			var quadPi = Mathf.PI / 4f;
			var absTheta = Mathf.Abs( theta );
			float radius;

			if( absTheta <= quadPi || absTheta >= 3 * quadPi )
			{
				radius = Mathf.Abs( delta.x );
			}
			else
			{
				radius = Mathf.Abs( delta.y );
			}

			// delta の大きさと円の半径の相似で極座標が求まる
			var magnitude = delta.magnitude;
			float ratio = 0;

			if( magnitude > 0 )
			{
				ratio = radius / magnitude;
			}

			delta *= ratio;
		}

		m_SwipeTransform.position = start + delta;
	}

	private float CalcSwipeMoveDistance( float originDistance )
	{
		// Arctanの曲線がスワイプ制約モデルに使えそうなので使っているだけ
		return Mathf.Atan( m_SwipeMoveReductionRate * originDistance ) * m_SwipeMoveLimitDistance / ( Mathf.PI / 2f );
	}

	/// <summary>
	/// スワイプ条件を満たしている場合、スワイプコールバックを呼び出す。
	/// </summary>
	private void CallSwipeEvent()
	{
		if( Time.time >= m_LimitTime )
			return;

		Vector2 dist = ( Vector2 )Input.mousePosition - m_StartPos;
		float dx = Mathf.Abs( dist.x );
		float dy = Mathf.Abs( dist.y );
		float requiredPx = m_WidthReference ? Screen.width * m_ValidWidth : Screen.height * m_ValidWidth;

		if( dy < dx )
		{
			if( requiredPx < dx )
				m_SwipeDir = Mathf.Sign( dist.x ) < 0 ? Vector2.left : Vector2.right;
		}
		else
		{
			if( requiredPx < dy )
				m_SwipeDir = Mathf.Sign( dist.y ) < 0 ? Vector2.down : Vector2.up;
		}

		if( m_SwipeDir != Vector2.zero )
		{
			EventUtility.SafeInvokeUnityEvent( m_OnSwipe, m_SwipeDir );
		}
	}

	/// <summary>
	/// 最初の位置に戻る。
	/// </summary>
	private void MoveToStartPosition()
	{
		if( m_IsMoveToStart )
			return;

		m_IsMoveToStart = true;
		m_SwipeTransform.DOMove( m_SwipeStartPos, m_SwipeBackDuration ).OnComplete( () =>
		{
			m_IsMoveToStart = false;
			m_IsSwiping = false;
		} );
	}

	#endregion

}