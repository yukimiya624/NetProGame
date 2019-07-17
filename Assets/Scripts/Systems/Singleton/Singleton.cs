using UnityEngine;

/// <summary>
/// シングルトンパターンを実装するための基底クラス。
/// </summary>
public abstract class Singleton<T> where T : class, new()
{

	public static T Instance
	{
		get
		{
			// 複数同時に生成しないようにロックする
			lock( m_LockObj )
			{
				if( m_Instance == null )
				{
					m_Instance = new T();
				}

				return m_Instance;
			}
		}
	}

	private static T m_Instance;
	private static System.Object m_LockObj = new System.Object();

	#region Method Protected

	protected Singleton()
	{
		OnConstructor();
	}

	~Singleton()
	{
		m_Instance = null;
	}

	protected virtual void OnConstructor()
	{
	}

	public virtual void Init()
	{
	}

	#endregion
}