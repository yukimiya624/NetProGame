using System;
using System.Text;
using Object = UnityEngine.Object;


/// <summary>
/// データシリアライザに共通の処理を定義する基底クラス。
/// </summary>
public abstract class BaseDataSerializer
{

	#region Field Private

	private string m_FolderPath;
	private string m_FileName;

	private StringBuilder m_Builder;
	private System.Object m_LockObject;

	#endregion



	#region Property Public

	public string FolderPath
	{
		get
		{
			return m_FolderPath;
		}
		set
		{
			m_FolderPath = value;
		}
	}

	public string FileName
	{
		get
		{
			return m_FileName;
		}
		set
		{
			m_FileName = value;
		}
	}

	#endregion



	#region Constructor

	protected BaseDataSerializer()
	{
		m_Builder = new StringBuilder();
		m_LockObject = new Object();
	}

	#endregion



	#region Method Abstract

	/// <summary>
	/// データの保存時に呼び出されます。
	/// 保持しているデータを文字列に変換する処理を実装する必要があります。
	/// </summary>
	public abstract string Serialize();

	/// <summary>
	/// データの読込時に呼び出されます。
	/// 受け取った文字列をデータに変換する処理を実装する必要があります。
	/// </summary>
	public abstract void Deserialize( string rawData );

	#endregion



	#region Method Public

	/// <summary>
	/// ファイルのパスを返します。
	/// 相対パスか絶対パスかは、保持しているフォルダパスに依ります。
	/// </summary>
	public string GetFileFullPath()
	{
		lock( m_LockObject )
		{
			m_Builder.Remove( 0, m_Builder.Length );
			m_Builder.Append( m_FolderPath ).Append( "/" ).Append( m_FileName );
			return m_Builder.ToString();
		}
	}

	/// <summary>
	/// データを保存します。
	/// このメソッドはファイル保存処理のラッパーに過ぎません。
	/// </summary>
	public void Save( Action onSuccess = null, Action onFailure = null )
	{
		FileIOManager.Instance.Save( GetFileFullPath(), this, onSuccess, onFailure );
	}

	/// <summary>
	/// データを読み込みます。
	/// このメソッドはファイル保存処理のラッパーに過ぎません。
	/// </summary>
	public void Load( Action onSuccess = null, Action onFailure = null )
	{
		FileIOManager.Instance.Load( GetFileFullPath(), this, onSuccess, onFailure );
	}

	/// <summary>
	/// データを暗号化して保存します。
	/// このメソッドはファイル保存処理のラッパーに過ぎません。
	/// </summary>
	public void EncryptSave( Action onSuccess = null, Action onFailure = null )
	{
		FileIOManager.Instance.EncryptSave( GetFileFullPath(), this, onSuccess, onFailure );
	}

	/// <summary>
	/// データを復号化して読み込みます。
	/// このメソッドはファイル保存処理のラッパーに過ぎません。
	/// </summary>
	public void EncryptLoad( Action onSuccess = null, Action onFailure = null )
	{
		FileIOManager.Instance.EncryptLoad( GetFileFullPath(), this, onSuccess, onFailure );
	}

	#endregion

}