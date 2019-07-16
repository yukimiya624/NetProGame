using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

/// <summary>
/// 暗号化及び復号化をサポートするクラス。
/// </summary>
public sealed class EncryptManager : GlobalSingletonMonoBehavior<EncryptManager>
{

	#region Definition

	/// <summary>
	/// 暗号化情報。
	/// </summary>
	public struct EncryptInfo
	{
		public byte[] Key;
		public byte[] Iv;
	}

	public const int IV_LENGTH = 16;
	public const int KEY_LENGTH = 16;

	private const string PASSWORD = "This passward is only validate in Unity editor. And must not chenge!";
	private const string SALT = "Must not chenge this salt!";

	#endregion



	#region Field Private

	private Rijndael m_Rijndael;
	private EncryptInfo m_EncryptInfo;

	#endregion



	#region Method Private

	private static byte[] GetPassword()
	{
		byte[] bytes;
#if UNITY_EDITOR
		bytes = Encoding.UTF8.GetBytes( PASSWORD );
#else
		bytes = Encoding.UTF8.GetBytes( SystemInfo.deviceUniqueIdentifier );
#endif
		return bytes;
	}

	private static byte[] GetSalt()
	{
		return Encoding.UTF8.GetBytes( SALT );
	}

	#endregion



	protected override void OnAwake()
	{
		base.OnAwake();
		m_Rijndael = Rijndael.Create();
		m_EncryptInfo = CreateEncryptInfo();
		m_Rijndael.IV = m_EncryptInfo.Iv;
		m_Rijndael.Key = m_EncryptInfo.Key;
	}



	#region Method Public

	/// <summary>
	/// 暗号化情報を生成して返す。
	/// </summary>
	public static EncryptInfo CreateEncryptInfo()
	{
		var byteGenerator = new Rfc2898DeriveBytes( GetPassword(), GetSalt(), 1000 );
		var random = new RNGCryptoServiceProvider();
		var info = new EncryptInfo();
		var iv = new byte[IV_LENGTH];

		random.GetBytes( iv );
		info.Iv = iv;
		info.Key = byteGenerator.GetBytes( KEY_LENGTH );
		return info;
	}

	/// <summary>
	/// 平文文字列を暗号化しbyte配列へ変換して返す。
	/// </summary>
	public byte[] EncryptToBytes( string origin )
	{
		using( ICryptoTransform encryptor = m_Rijndael.CreateEncryptor() )
		{
			byte[] src = Encoding.UTF8.GetBytes( origin );
			return encryptor.TransformFinalBlock( src, 0, src.Length );
		}
	}

	/// <summary>
	/// 平文文字列を暗号化しBase64文字列へ変換して返す。
	/// </summary>
	public string EncryptToBase64( string origin )
	{
		return Convert.ToBase64String( EncryptToBytes( origin ) );
	}

	/// <summary>
	/// 暗号化されているbyte配列を復号して平文文字列として返す。
	/// </summary>
	public string DecryptFromBytes( byte[] encrypted )
	{
		using( ICryptoTransform decryptor = m_Rijndael.CreateDecryptor() )
		{
			byte[] decryptedBytes = decryptor.TransformFinalBlock( encrypted, 0, encrypted.Length );
			return Encoding.UTF8.GetString( decryptedBytes );
		}
	}

	/// <summary>
	/// 暗号化されているBase64文字列を復号して平文文字列として返す。
	/// </summary>
	public string DecryptFromBase64( string encrypted )
	{
		byte[] src = Convert.FromBase64String( encrypted );
		return DecryptFromBytes( src );
	}

	#endregion
}