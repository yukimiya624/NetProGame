using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
	using UnityEditor;
	using System.Text;
	using System.IO;
	using System;
#endif


/// <summary>
/// 多言語に対応した文言を管理するクラス。
/// </summary>
public class TranslateManager : GlobalSingletonMonoBehavior<TranslateManager>
{

	#region Field Inspector

	[SerializeField]
	private TranslateLanguage m_DefaultLanguage;

	[SerializeField]
	private TranslateTextSet m_DefaultTextSet;

	[SerializeField]
	private List<TranslateTextSet> m_TranslaterTextSets;

	[SerializeField]
	private UnityEvent m_OnChangeTranslateLanguage = new UnityEvent();

	[SerializeField]
	private TranslateLanguage m_CurrentTranslateLanguage;

	#endregion



	#region Property Public

	public TranslateLanguage DefaultLanguage
	{
		get
		{
			return m_DefaultLanguage;
		}
	}

	public UnityEvent OnChangeTranslateLanguage
	{
		get
		{
			return m_OnChangeTranslateLanguage;
		}
	}

	public TranslateLanguage CurrentTranslateLanguage
	{
		get
		{
			return m_CurrentTranslateLanguage;
		}
	}

	#endregion



	#region Method Public

	public TranslateLanguage GetLanguageFromSystemLanguage( SystemLanguage language )
	{
		switch( language )
		{
			case SystemLanguage.English:
				return TranslateLanguage.English;

			case SystemLanguage.Japanese:
				return TranslateLanguage.Japanese;

			case SystemLanguage.Chinese:
				return TranslateLanguage.ChineseSimplified;

			case SystemLanguage.ChineseSimplified:
				return TranslateLanguage.ChineseSimplified;

			case SystemLanguage.ChineseTraditional:
				return TranslateLanguage.ChineseTraditional;

			case SystemLanguage.Korean:
				return TranslateLanguage.Korean;

			case SystemLanguage.Russian:
				return TranslateLanguage.Russian;

			case SystemLanguage.Italian:
				return TranslateLanguage.Italian;

			case SystemLanguage.French:
				return TranslateLanguage.French;

			case SystemLanguage.German:
				return TranslateLanguage.German;

			case SystemLanguage.Spanish:
				return TranslateLanguage.Spanish;

			case SystemLanguage.Portuguese:
				return TranslateLanguage.Portuguese;

			default:
				return TranslateLanguage.English;
		}
	}


	public void ChangeTranslateLanguage( TranslateLanguage newLanguage )
	{
		if( newLanguage == m_CurrentTranslateLanguage )
			return;

		m_CurrentTranslateLanguage = newLanguage;

		EventUtility.SafeInvokeUnityEvent( m_OnChangeTranslateLanguage );
	}

	public TextSet GetTextSet( string key )
	{
		return GetTextSet( key, m_CurrentTranslateLanguage );
	}

	public TextSet GetTextSet( string key, TranslateLanguage targetLanguage )
	{

		if( string.IsNullOrEmpty( key ) )
			return null;

		TranslateTextSet translateSet = null;

		foreach( var info in m_TranslaterTextSets )
		{
			if( info == null )
				continue;

			if( info.TextSetKey == key )
			{
				translateSet = info;
				break;
			}
		}

		if( translateSet == null )
			return null;

		var translateSetInfo = translateSet.GetTextInfo( m_CurrentTranslateLanguage, m_DefaultLanguage );

		if( translateSetInfo == null )
			return null;

		if( translateSetInfo.UseDefaultTextSet )
		{
			var defaultSetInfo = m_DefaultTextSet.GetTextInfo( m_CurrentTranslateLanguage, m_DefaultLanguage );

			if( defaultSetInfo == null )
				return translateSetInfo.TextSet;

			defaultSetInfo.TextSet.Text = translateSetInfo.TextSet.Text;
			return defaultSetInfo.TextSet;
		}

		return translateSetInfo.TextSet;
	}

	public int GetIDFromLanguage( TranslateLanguage language )
	{
		switch( language )
		{
			case TranslateLanguage.English:
				return 0;

			case TranslateLanguage.Japanese:
				return 1;

			case TranslateLanguage.ChineseSimplified:
				return 2;

			case TranslateLanguage.ChineseTraditional:
				return 3;

			case TranslateLanguage.French:
				return 4;

			case TranslateLanguage.Italian:
				return 5;

			case TranslateLanguage.German:
				return 6;

			case TranslateLanguage.Korean:
				return 7;

			case TranslateLanguage.Portuguese:
				return 8;

			case TranslateLanguage.Russian:
				return 9;

			case TranslateLanguage.Spanish:
				return 10;

			case TranslateLanguage.Hindi:
				return 11;

			default:
				return -1;
		}
	}

	public TranslateLanguage GetLanguageFromID( int languageID )
	{
		switch( languageID )
		{
			case 0:
				return TranslateLanguage.English;

			case 1:
				return TranslateLanguage.Japanese;

			case 2:
				return TranslateLanguage.ChineseSimplified;

			case 3:
				return TranslateLanguage.ChineseTraditional;

			case 4:
				return TranslateLanguage.French;

			case 5:
				return TranslateLanguage.Italian;

			case 6:
				return TranslateLanguage.German;

			case 7:
				return TranslateLanguage.Korean;

			case 8:
				return TranslateLanguage.Portuguese;

			case 9:
				return TranslateLanguage.Russian;

			case 10:
				return TranslateLanguage.Spanish;

			case 11:
				return TranslateLanguage.Hindi;

			default:
				return m_DefaultLanguage;
		}
	}

	#endregion



#if UNITY_EDITOR

	[CustomEditor( typeof( TranslateManager ) )]
	public class TranslateManagerEditor : Editor
	{

		/// <summary>
		/// 無効な文字の配列
		/// </summary>
		private static readonly string[] INVALUD_CHARS =
		{
			" ", "!", "\"", "#", "$",
			"%", "&", "\'", "(", ")",
			"-", "=", "^", "~", "\\",
			"|", "[", "{", "@", "`",
			"]", "}", ":", "*", ";",
			"+", "/", "?", ".", ">",
			",", "<"
		};

		private static string m_GeneratePath = "Assets/Constant/";

		private static TranslateLanguage m_TargetLanguage;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUILayout.Space();
			m_GeneratePath = EditorGUILayout.TextField( "Generate Path", m_GeneratePath );

			if( string.IsNullOrEmpty( m_GeneratePath ) )
			{
				m_GeneratePath = "Assets/Constant/";
			}
			else
			{
				string firstDirectory = m_GeneratePath;
				int idx = m_GeneratePath.IndexOf( "/" );

				if( idx >= 0 )
				{
					firstDirectory = m_GeneratePath.Substring( 0, idx );
				}

				if( firstDirectory != "Assets" )
				{
					m_GeneratePath = "Assets" + m_GeneratePath.Replace( firstDirectory, "" );
				}
			}

			EditorGUILayout.Space();

			if( GUILayout.Button( "Constantize TextSet Key" ) )
			{
				if( !CanGenerate() )
				{
					Debug.LogError( "It cannot Constantize textSet key now!" );
					return;
				}

				if( ConstantizeTextSetKeys( target as TranslateManager ) )
				{
					EditorUtility.DisplayDialog( "TranslateManager", "定数化が成功しました", "OK" );
				}
				else
				{
					EditorUtility.DisplayDialog( "TranslateManager", "定数化が失敗しました", "OK" );
				}
			}

			EditorGUILayout.Space();

			m_TargetLanguage = ( TranslateLanguage )EditorGUILayout.EnumPopup( "Change Target Language", m_TargetLanguage );

			if( GUILayout.Button( "Change Language" ) )
			{
				( target as TranslateManager ).ChangeTranslateLanguage( m_TargetLanguage );
			}
		}

		/// <summary>
		/// プレハブ名を定数で管理するクラスを作成できるかどうかを取得します。
		/// </summary>
		private static bool CanGenerate()
		{
			return !EditorApplication.isPlaying && !Application.isPlaying && !EditorApplication.isCompiling;
		}

		private static bool ConstantizeTextSetKeys( TranslateManager translateManager )
		{
			try
			{
				var builder = new StringBuilder();
				builder.AppendLine( "/// <summary>" );
				builder.AppendLine( "/// 多言語対応のテキストセットキーを定数として保持するクラス。" );
				builder.AppendLine( "/// </summary>" );
				builder.AppendFormat( "public static class TranslateTextSetKey" ).AppendLine();
				builder.AppendLine( "{" );

				foreach( var textSet in translateManager.m_TranslaterTextSets )
				{
					if( textSet == null )
						continue;

					string name = RemoveInvalidChars( textSet.TextSetKey );
					builder.Append( "\t" ).AppendFormat( @"public const string {0} = ""{1}"";", name, textSet.TextSetKey ).AppendLine();
				}

				builder.AppendLine( "}" );

				var path = Path.Combine( m_GeneratePath, "TranslateTextSetKey.cs" );
				var directoryName = Path.GetDirectoryName( path );

				if( !Directory.Exists( directoryName ) )
				{
					Directory.CreateDirectory( directoryName );
				}

				File.WriteAllText( path, builder.ToString(), Encoding.UTF8 );
				AssetDatabase.Refresh( ImportAssetOptions.ImportRecursive );
			}
			catch( Exception e )
			{
				Debug.LogException( e );
				return false;
			}

			return true;
		}

		/// <summary>
		/// 無効な文字を削除します。
		/// </summary>
		private static string RemoveInvalidChars( string str )
		{
			Array.ForEach( INVALUD_CHARS, c => str = str.Replace( c, string.Empty ) );
			return str;
		}

	}

#endif

}