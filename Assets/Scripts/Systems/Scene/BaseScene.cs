using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

/// <summary>
/// シーンごとのマネージャ等を管理するコンポーネント。
/// </summary>
public class BaseScene : ControllableMonoBehavior
{
	/// <summary>
	/// このシーンが何のシーンなのか。
	/// </summary>
	[SerializeField]
	private BaseSceneManager.E_SCENE m_Scene;

	/// <summary>
	/// このシーンのサイクル。
	/// </summary>
	[SerializeField]
	private BaseSceneManager.E_SCENE_CYCLE m_SceneCycle;

	/// <summary>
	/// このシーンに固有で紐づいているマネージャのリスト。
	/// </summary>
	[SerializeField, Tooltip( "シーンに固有で紐づいているマネージャは、ここにアタッチして下さい。なお、アタッチした順番通りにマネージャは実行されることに注意して下さい。" )]
	private List<ControllableMonoBehavior> m_ManagerList;



	#region Getter & Setter

	/// <summary>
	/// このシーンの値を取得する。
	/// </summary>
	public BaseSceneManager.E_SCENE GetScene()
	{
		return m_Scene;
	}

	/// <summary>
	/// このシーンのサイクルを取得する。
	/// </summary>
	public BaseSceneManager.E_SCENE_CYCLE GetSceneCycle()
	{
		return m_SceneCycle;
	}

	/// <summary>
	/// このシーンのサイクルを設定する。
	/// </summary>
	public void SetSceneCycle( BaseSceneManager.E_SCENE_CYCLE value )
	{
		m_SceneCycle = value;
	}

	/// <summary>
	/// このシーンに固有で紐づいているマネージャのリストを取得する。。
	/// </summary>
	public List<ControllableMonoBehavior> GetManagerList()
	{
		return m_ManagerList;
	}

	#endregion

	/// <summary>
	/// Unityで制御される生成直後に呼び出される処理。
	/// GameManagerが初期化されていない場合、最初にあるシーンに強制的に遷移する。
	/// </summary>
	protected override void Awake()
	{
		if( !GameManager.CheckExistInstance() )
		{
			BaseSceneManager.SetBeginScene( GetScene() );
			SceneManager.LoadScene( 0 );
			return;
		}

		BaseSceneManager.Instance.RegisterScene( this );
		OnAwake();
	}

	/// <summary>
	/// Unityで制御される破棄直前に呼び出される処理。
	/// </summary>
	protected override void OnDestroy()
	{
		OnDestroyed();
	}

	/// <summary>
	/// インスタンス生成直後に呼び出される処理。
	/// </summary>
	protected override void OnAwake()
	{
	}

	/// <summary>
	/// インスタンス破棄直前に呼び出される処理。
	/// </summary>
	protected override void OnDestroyed()
	{
		if( m_ManagerList != null )
		{
			m_ManagerList.Clear();
		}

		m_ManagerList = null;
	}

	/// <summary>
	/// BaseSceneManagerからこのメソッドが呼び出されることはありません。
	/// 代わりにOnBeforeShotやOnAfterShowを使用して下さい。
	/// </summary>
	public sealed override void OnInitialize()
	{
	}

	/// <summary>
	/// BaseSceneManagerからこのメソッドが呼び出されることはありません。
	/// 代わりにOnBeforeHideやOnAfterHideを使用して下さい。
	/// </summary>
	public sealed override void OnFinalize()
	{
	}

	public override void OnStart()
	{
		m_ManagerList.ForEach( ( m ) => m.OnStart() );
	}

	public override void OnUpdate()
	{
		m_ManagerList.ForEach( ( m ) => m.OnUpdate() );
	}

	public override void OnLateUpdate()
	{
		m_ManagerList.ForEach( ( m ) => m.OnLateUpdate() );
	}

	public override void OnFixedUpdate()
	{
		m_ManagerList.ForEach( ( m ) => m.OnFixedUpdate() );
	}

	/// <summary>
	/// シーン遷移前の演出が入る直前に呼び出される処理。
	/// </summary>
	public virtual void OnBeforeHide( Action onComplete )
	{
		EventUtility.SafeInvokeAction( onComplete );
	}

	/// <summary>
	/// シーン遷移前の演出が入る直前に呼び出される処理。
	/// </summary>
	public virtual void OnAfterHide( Action onComplete )
	{
		EventUtility.SafeInvokeAction( onComplete );
	}

	/// <summary>
	/// シーン遷移前の演出が入る直前に呼び出される処理。
	/// </summary>
	public virtual void OnBeforeShow( Action onComplete )
	{
		EventUtility.SafeInvokeAction( onComplete );
	}

	/// <summary>
	/// シーン遷移前の演出が入る直前に呼び出される処理。
	/// </summary>
	public virtual void OnAfterShow( Action onComplete )
	{
		EventUtility.SafeInvokeAction( onComplete );
	}

	public void OnInitializeManagers()
	{
		m_ManagerList.ForEach( ( m ) => m.OnInitialize() );
	}

	public void OnFinalizeManagers()
	{
		m_ManagerList.ForEach( ( m ) => m.OnFinalize() );
	}
}