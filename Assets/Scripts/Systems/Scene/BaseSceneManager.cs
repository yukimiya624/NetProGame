using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// シーン遷移およびシーンのごとのマネージャを管理する。
/// </summary>
public class BaseSceneManager : SingletonMonoBehavior<BaseSceneManager>
{
	/// <summary>
	/// 各BaseSceneに割り当てられる列挙値。
	/// </summary>
	[Serializable]
	public enum E_SCENE
	{
		DEFAULT = -1,
		PRE_LAUNCH = 0,
        TITLE = 1,
        MATCH = 2,
        BATTLE = 3,
        RESULT = 4,
	}

	/// <summary>
	/// BaseSceneのサイクル。
	/// </summary>
	public enum E_SCENE_CYCLE
	{
		/// <summary>
		/// 他のシーンから遷移してきた直後。
		/// まだゲームサイクルに乗っていない状態。
		/// </summary>
		IN,

		/// <summary>
		/// シーン遷移が完了してきてから最初のフレームで呼び出されるまで。
		/// </summary>
		STANDBY,

		/// <summary>
		/// ゲームサイクルに乗っている状態。
		/// </summary>
		UPDATE,

		/// <summary>
		/// ゲームサイクルから外れ、他のシーンに遷移していく状態。
		/// </summary>
		OUT,
	}

	/// <summary>
	/// シーン遷移時の情報を保持する。
	/// </summary>
	[Serializable]
	public struct TransitionInfo
	{
		public E_SCENE CurrentScene;
		public E_SCENE NextScene;
		public List<E_SCENE> AdditiveScene;
		//public FadeOutBase FadeOutEffect;
		//public LoadingBase LoadingEffect;
		//public FadeInBase FadeInEffect;
	}

	/// <summary>
	/// Unityのプレイ開始時にいたシーンの値。
	/// </summary>
	private static E_SCENE ms_BeginScene;

	const float MAX_LOAD_PROGRESS = 0.9f;

	[SerializeField, Tooltip( "起動した瞬間にいたシーンから始めるかどうか" )]
	private bool m_IsStartFromBeginningScene;

	[SerializeField, Tooltip( "PreLaunchシーンから最初に遷移するシーン" )]
	private E_SCENE m_StartScene;

	[SerializeField]
	List<TransitionInfo> m_TransitionInfos;

	[SerializeField]
	private E_SCENE m_CurrentInfoNextScene;

	[SerializeField]
	private List<E_SCENE> m_CurrentInfoNextAdditiveScenes;

	[SerializeField]
	private BaseScene m_CurrentScene;

	[SerializeField]
	private List<BaseScene> m_CurrentAdditiveScenes;

    /// <summary>
    /// 起動した瞬間にいたシーンから始めるかどうかを取得する。
    /// </summary>
    /// <returns></returns>
    public bool IsStartFromBeginningScene()
    {
        return m_IsStartFromBeginningScene;
    }

	/// <summary>
	/// Unityのプレイ開始時にいたシーンの値を設定する。
	/// </summary>
	public static void SetBeginScene( E_SCENE value )
	{
		ms_BeginScene = value;
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
	}

	/// <summary>
	/// このコンポーネントの初期化処理。
	/// OnAwakeとは異なる。
	/// このメソッドの呼び出しは、Unityではなく任意の制御下で行われる。
	/// </summary>
	public override void OnInitialize()
	{
	}

	/// <summary>
	/// このコンポーネントの終了処理。
	/// OnDestroyedとは異なる。
	/// このメソッドの呼び出しは、Unityではなく任意の制御下で行われる。
	/// </summary>
	public override void OnFinalize()
	{
	}

	/// <summary>
	/// 最初のフレームで呼び出される処理。
	/// このメソッドの呼び出しは、Unityではなく任意の制御下で行われる。
	/// </summary>
	public override void OnStart()
	{
	}

	/// <summary>
	/// 毎フレームで呼び出される処理。
	/// このメソッドの呼び出しは、Unityではなく任意の制御下で行われる。
	/// </summary>
	public override void OnUpdate()
	{
		SceneGameCycle( E_SCENE_CYCLE.STANDBY, ( scene ) =>
		{
			scene.OnStart();
			scene.SetSceneCycle( E_SCENE_CYCLE.UPDATE );
		} );

		SceneGameCycle( E_SCENE_CYCLE.UPDATE, ( scene ) => scene.OnUpdate() );
	}

	/// <summary>
	/// OnUpdateの後に呼び出される処理。
	/// このメソッドの呼び出しは、Unityではなく任意の制御下で行われる。
	/// </summary>
	public override void OnLateUpdate()
	{
		SceneGameCycle( E_SCENE_CYCLE.UPDATE, ( scene ) => scene.OnLateUpdate() );
	}

	public override void OnFixedUpdate()
	{
		SceneGameCycle( E_SCENE_CYCLE.UPDATE, ( scene ) => scene.OnFixedUpdate() );
	}




	/// <summary>
	/// シーン遷移する。
	/// </summary>
	/// <param name="nextScene">遷移先のシーン値</param>
	public void LoadScene( E_SCENE nextScene )
	{
		int nowIndex = SceneManager.GetActiveScene().buildIndex;
		int nextIndex = ( int )nextScene;

		// デフォルト値を指定している場合は無視
		if( nextIndex < 0 )
		{
			return;
		}

		// 現在のシーンと指定シーンとの組み合わせを探す
		foreach( var info in m_TransitionInfos )
		{
			if( nowIndex == ( int )info.CurrentScene && nextScene == info.NextScene )
			{
				StartCoroutine( LoadSceneSequence( info, nextScene, info.AdditiveScene ) );
				return;
			}
		}

		// 現在のシーンをワイルドカードとして指定シーンとの組み合わせを探す
		foreach( var info in m_TransitionInfos )
		{
			if( info.CurrentScene < 0 && nextScene == info.NextScene )
			{
				StartCoroutine( LoadSceneSequence( info, nextScene, info.AdditiveScene ) );
				return;
			}
		}

		// 現在のシーンと指定したシーンをワイルドカードとして組み合わせを探す
		foreach( var info in m_TransitionInfos )
		{
			if( nowIndex == ( int )info.CurrentScene && info.NextScene < 0 )
			{
				StartCoroutine( LoadSceneSequence( info, nextScene, null ) );
				return;
			}
		}

		// 現在のシーンも指定シーンもワイルドカードとして組み合わせを探す
		foreach( var info in m_TransitionInfos )
		{
			if( info.CurrentScene < 0 && info.NextScene < 0 )
			{
				StartCoroutine( LoadSceneSequence( info, nextScene, null ) );
				return;
			}
		}

		// 完全に見つからなかった場合
		SceneManager.LoadScene( nextIndex );
	}

	/// <summary>
	/// シーンをマネージャに登録する。
	/// </summary>
	/// <param name="scene">登録するシーンコンポーネント</param>
	public void RegisterScene( BaseScene scene )
	{
		if( scene == null )
		{
			return;
		}

		if( scene.GetScene() == m_CurrentInfoNextScene )
		{
			m_CurrentScene = scene;
			return;
		}

		if( m_CurrentInfoNextAdditiveScenes.Contains( scene.GetScene() ) && !m_CurrentAdditiveScenes.Contains( scene ) )
		{
			m_CurrentAdditiveScenes.Add( scene );
			return;
		}
	}

	/// <summary>
	/// シーン遷移シーケンス処理。
	/// </summary>
	/// <param name="info">遷移情報</param>
	/// <param name="nextScene">遷移先のシーン値</param>
	/// <param name="nextAdditiveScenes">遷移先のサブシーン値のリスト</param>
	/// <returns></returns>
	private IEnumerator LoadSceneSequence( TransitionInfo info, E_SCENE nextScene, List<E_SCENE> nextAdditiveScenes = null )
	{

		if( nextScene < 0 )
		{
			yield break;
		}

		m_CurrentInfoNextScene = nextScene;
		m_CurrentInfoNextAdditiveScenes = nextAdditiveScenes;

		//FadeOutBase fadeOut = info.FadeOutEffect;
		//FadeInBase fadeIn = info.FadeInEffect;

		SetSceneCycle( E_SCENE_CYCLE.OUT );
		yield return OnBeforeTransition( ( scene, callback ) => scene.OnBeforeHide( callback ) );

		// フェードアウト処理
		//if( fadeOut != null )
		//{
		//	var obj = Instantiate( info.FadeOutEffect );
		//	yield return StartCoroutine( obj.FadeOut() );
		//	Destroy( obj );
		//}

		////ロード画面生成
		//var loadingObj = Instantiate( info.LoadingEffect );

		//bool isCompleteTransition = false;
		//TransitionManager.Instance.Hide( () => isCompleteTransition = true );
		//yield return new WaitUntil( () => isCompleteTransition );

		yield return OnBeforeTransition( ( scene, callback ) => scene.OnAfterHide( callback ) );

		m_CurrentScene = null;
		m_CurrentAdditiveScenes = new List<BaseScene>();

		List<AsyncOperation> asyncList = OnLoadScene();

		yield return WaitForLoadingScenes( asyncList );
		yield return WaitForRegisteringScenes();

		//ロード完了通知
		//loadingObj.OnLoadComplete();

		SetSceneCycle( E_SCENE_CYCLE.IN );
		yield return OnAfterTransition( ( scene, callback ) => scene.OnBeforeShow( callback ) );

		//フェードイン処理
		//if( fadeIn != null )
		//{
		//	var obj = Instantiate( info.FadeInEffect );
		//	yield return StartCoroutine( obj.FadeIn() );
		//	Destroy( obj );
		//}

		//isCompleteTransition = false;
		//TransitionManager.Instance.Show( () => isCompleteTransition = true );
		//yield return new WaitUntil( () => isCompleteTransition );

		yield return OnAfterTransition( ( scene, callback ) => scene.OnAfterShow( callback ) );
		SetSceneCycle( E_SCENE_CYCLE.STANDBY );
	}

	/// <summary>
	/// 現在のシーンの全てのサイクルを設定する。
	/// </summary>
	private void SetSceneCycle( E_SCENE_CYCLE value )
	{
		if( m_CurrentAdditiveScenes != null )
		{
			foreach( var scene in m_CurrentAdditiveScenes )
			{
				if( scene == null )
				{
					continue;
				}

				scene.SetSceneCycle( value );
			}
		}

		if( m_CurrentScene != null )
		{
			m_CurrentScene.SetSceneCycle( value );
		}
	}

	/// <summary>
	/// シーン遷移前の処理。
	/// </summary>
	private IEnumerator OnBeforeTransition( Action<BaseScene, Action> callback )
	{
		if( m_CurrentAdditiveScenes != null )
		{
			foreach( var scene in m_CurrentAdditiveScenes )
			{
				if( scene == null )
				{
					continue;
				}

				bool isComplete = false;
				EventUtility.SafeInvokeAction( callback, scene, () => isComplete = true );
				yield return new WaitUntil( () => isComplete );
			}
		}

		if( m_CurrentScene != null )
		{
			bool isComplete = false;
			EventUtility.SafeInvokeAction( callback, m_CurrentScene, () => isComplete = true );
			yield return new WaitUntil( () => isComplete );
		}
	}

	/// <summary>
	/// シーン遷移前の処理。
	/// </summary>
	private IEnumerator OnAfterTransition( Action<BaseScene, Action> callback )
	{
		if( m_CurrentScene != null )
		{
			bool isComplete = false;
			EventUtility.SafeInvokeAction( callback, m_CurrentScene, () => isComplete = true );
			yield return new WaitUntil( () => isComplete );
		}

		if( m_CurrentAdditiveScenes != null )
		{
			foreach( var scene in m_CurrentAdditiveScenes )
			{
				if( scene == null )
				{
					continue;
				}

				bool isComplete = false;
				EventUtility.SafeInvokeAction( callback, scene, () => isComplete = true );
				yield return new WaitUntil( () => isComplete );
			}
		}
	}

	/// <summary>
	/// シーン読込処理。
	/// </summary>
	private List<AsyncOperation> OnLoadScene()
	{
		List<AsyncOperation> asyncList = new List<AsyncOperation>();
		asyncList.Add( SceneManager.LoadSceneAsync( ( int )m_CurrentInfoNextScene ) );

		if( m_CurrentInfoNextAdditiveScenes != null )
		{
			foreach( var scene in m_CurrentInfoNextAdditiveScenes )
			{
				if( scene < 0f )
				{
					continue;
				}

				asyncList.Add( SceneManager.LoadSceneAsync( ( int )scene, LoadSceneMode.Additive ) );
			}
		}

		return asyncList;
	}

	/// <summary>
	/// 遷移先の全てのシーンの読込が完了するまで待機する。
	/// </summary>
	private IEnumerator WaitForLoadingScenes( List<AsyncOperation> asyncList )
	{
		//m_ProgressProperty.Value = 0f;

		while( true )
		{
			yield return null;

			bool isDone = true;
			float progress = 0;

			foreach( var async in asyncList )
			{
				if( !async.isDone )
				{
					isDone = false;
				}

				progress += async.progress;
			}

			progress /= asyncList.Count;
			//m_ProgressProperty.Value = progress;

			if( isDone )
			{
				break;
			}
		}
	}

	/// <summary>
	/// 遷移先の全てのシーンがマネージャに登録し終わるまで待機する。
	/// </summary>
	private IEnumerator WaitForRegisteringScenes()
	{
		while( true )
		{
			yield return null;

			if( m_CurrentScene == null )
			{
				continue;
			}

			if( m_CurrentAdditiveScenes != null && m_CurrentInfoNextAdditiveScenes != null )
			{
				if( m_CurrentAdditiveScenes.Count < m_CurrentInfoNextAdditiveScenes.Count )
				{
					continue;
				}
			}

			break;
		}
	}

	/// <summary>
	/// 現在のシーンに対して、StartやUpdate、LateUpdate等を汎用的に呼び出すための処理。
	/// </summary>
	/// <param name="cycle">シーンの状態</param>
	/// <param name="callback">ここで指定したコールバックの中で各シーンに対するサイクルメソッドを呼び出す</param>
	private void SceneGameCycle( E_SCENE_CYCLE cycle, Action<BaseScene> callback )
	{
		if( m_CurrentScene != null && m_CurrentScene.GetSceneCycle() == cycle )
		{
			EventUtility.SafeInvokeAction( callback, m_CurrentScene );
		}

		foreach( var addScene in m_CurrentAdditiveScenes )
		{
			if( addScene == null || addScene.GetSceneCycle() != cycle )
			{
				continue;
			}

			EventUtility.SafeInvokeAction( callback, addScene );
		}
	}

	public void LoadOnGameStart()
	{
		if( m_IsStartFromBeginningScene && ms_BeginScene != E_SCENE.PRE_LAUNCH)
        {
            LoadScene(ms_BeginScene);
        }
		else
        {
            LoadScene(m_StartScene);
        }
	}
}
