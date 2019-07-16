using System;
using System.IO;
using System.Security.Cryptography;
using JetBrains.Annotations;

/// <summary>
/// ファイルの入出力をサポートするクラス。
/// 暗号化しない方法と暗号化する方法をそれぞれサポートしています。
/// </summary>
public sealed class FileIOManager : GlobalSingletonMonoBehavior<FileIOManager>
{

	#region Field Private

	/// <summary>
	/// セーブまたはロード時に処理が重複しないようにロックするためのオブジェクト。
	/// </summary>
	private System.Object m_lockObject = new System.Object();

	#endregion



	#region Method Public

	/// <summary>
	/// DataStoreのデータをシリアライズして指定されたファイルに保存します。
	/// 指定されたファイルが存在している場合は上書き、存在していない場合は新規作成します。
	/// </summary>
	public void Save( string fullPath, BaseDataSerializer dataSerializer, Action onSuccess = null, Action onFailure = null )
	{
		lock( m_lockObject )
		{
			try
			{
				if( File.Exists( fullPath ) )
				{
					File.Delete( fullPath );
				}

				string rawData = dataSerializer.Serialize();

				using( StreamWriter writer = GetStreamWriter( fullPath ) )
				{
					writer.Write( rawData );
				}
			}
			catch( Exception e )
			{
#if DEBUG_ON
				Debug.LogException( e );
#endif
				EventUtility.SafeInvokeAction( onFailure );
				return;
			}

			EventUtility.SafeInvokeAction( onSuccess );
		}
	}

	/// <summary>
	/// 指定されたファイルを読み取りDataStoreにデータをデシリアライズします。
	/// 指定されたファイルが存在していない場合は例外を発生させます。
	/// </summary>
	public void Load( string fileFullPath, BaseDataSerializer dataSerializer, Action onSuccess = null, Action onFailure = null )
	{
		lock( m_lockObject )
		{
			try
			{
				if( !File.Exists( fileFullPath ) )
				{
					throw new IOException( string.Format( "指定されたファイルが存在しません！ 指定されたファイル = {0}", fileFullPath ) );
				}

				using( StreamReader reader = GetStreamReader( fileFullPath ) )
				{
					dataSerializer.Deserialize( reader.ReadToEnd() );
				}
			}
			catch( Exception e )
			{
#if DEBUG_ON
				Debug.LogException( e );
#endif
				EventUtility.SafeInvokeAction( onFailure );
				return;
			}

			EventUtility.SafeInvokeAction( onSuccess );
		}
	}

	/// <summary>
	/// DataStoreのデータをシリアライズして指定されたファイルに暗号化して保存します。
	/// 指定されたファイルが存在している場合は上書き、存在していない場合は新規作成します。
	/// </summary>
	public void EncryptSave( string fullPath, BaseDataSerializer dataSerializer, Action onSuccess = null, Action onFailure = null )
	{
		lock( m_lockObject )
		{
			try
			{
				if( File.Exists( fullPath ) )
				{
					File.Delete( fullPath );
				}

				string rawData = dataSerializer.Serialize();

				using( var writer = GetEncryptedStreamWriter( fullPath ) )
				{
					writer.Write( rawData );
				}
			}
			catch( Exception e )
			{
#if DEBUG_ON
				Debug.LogException( e );
#endif
				EventUtility.SafeInvokeAction( onFailure );
				return;
			}

			EventUtility.SafeInvokeAction( onSuccess );
		}
	}

	/// <summary>
	/// 指定されたファイルを読み取り復号化してDataStoreにデータをデシリアライズします。
	/// 指定されたファイルが存在していない場合は例外を発生させます。
	/// </summary>
	public void EncryptLoad( string fileFullPath, BaseDataSerializer dataSerializer, Action onSuccess = null, Action onFailure = null )
	{
		lock( m_lockObject )
		{
			try
			{
				if( !File.Exists( fileFullPath ) )
				{
					throw new IOException( string.Format( "指定されたファイルが存在しません！ 指定されたファイル = {0}", fileFullPath ) );
				}

				using( StreamReader reader = GetEncryptedStreamReader( fileFullPath ) )
				{
					dataSerializer.Deserialize( reader.ReadToEnd() );
				}
			}
			catch( Exception e )
			{
#if DEBUG_ON
				Debug.LogException( e );
#endif
				EventUtility.SafeInvokeAction( onFailure );
				return;
			}

			EventUtility.SafeInvokeAction( onSuccess );
		}
	}

	#endregion



	#region Method Private

	private StreamWriter GetStreamWriter( string fileFullPath )
	{
		SafeDirectoryGenerator.GenerateDirectory( fileFullPath );
		return new StreamWriter( new FileStream( fileFullPath, FileMode.OpenOrCreate ) );
	}

	private StreamReader GetStreamReader( string fileFullPath )
	{
		SafeDirectoryGenerator.GenerateDirectory( fileFullPath );
		return new StreamReader( new FileStream( fileFullPath, FileMode.Open ) );
	}

	private StreamWriter GetEncryptedStreamWriter( string fileFullPath )
	{
		SafeDirectoryGenerator.GenerateDirectory( fileFullPath );
		var underlyingStream = new FileStream( fileFullPath, FileMode.OpenOrCreate );

		var rijndael = Rijndael.Create();
		var encryptInfo = EncryptManager.CreateEncryptInfo();
		rijndael.Key = encryptInfo.Key;
		rijndael.IV = encryptInfo.Iv;

		// ファイルの先頭に初期ベクトルを追加
		underlyingStream.Write( encryptInfo.Iv, 0, encryptInfo.Iv.Length );
		var encryptedStream =
		    new CryptoStream( underlyingStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write );
		return new StreamWriter( encryptedStream );
	}

	private StreamReader GetEncryptedStreamReader( string fileFullPath )
	{
		SafeDirectoryGenerator.GenerateDirectory( fileFullPath );
		var underlyingStream = new FileStream( fileFullPath, FileMode.Open );

		byte[] iv = new byte[EncryptManager.IV_LENGTH];

		// ファイルの先頭から初期ベクトルを取得
		underlyingStream.Read( iv, 0, EncryptManager.IV_LENGTH );

		var rijndael = Rijndael.Create();
		var encryptInfo = EncryptManager.CreateEncryptInfo();
		rijndael.Key = encryptInfo.Key;
		rijndael.IV = iv;

		var encryptedStream = new CryptoStream( underlyingStream, rijndael.CreateDecryptor(), CryptoStreamMode.Read );
		return new StreamReader( encryptedStream );
	}

	#endregion

}