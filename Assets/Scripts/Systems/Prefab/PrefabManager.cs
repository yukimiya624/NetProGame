using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
	using UnityEditor;
	using System.Linq;
	using System.Text;
	using System.IO;
#endif

/// <summary>
/// プレハブの管理を一元管理します。
/// </summary>
public class PrefabManager : GlobalSingletonMonoBehavior<PrefabManager>
{

	#region Field Inspector

	[SerializeField]
	private List<PrefabInfo> m_PrefabInfos = null;

	#endregion



	#region Property Internal

	internal List<PrefabInfo> PrefabInfos
	{
		get
		{
			return m_PrefabInfos;
		}
	}

	#endregion



	#region Method Public

	/// <summary>
	/// マネージャが保持しているオリジナルのプレハブを返します。
	/// 該当するプレハブが見つからない場合は、nullを返します。
	/// </summary>
	public GameObject GetOriginObject( string prefabName )
	{
		if( m_PrefabInfos == null )
		{
#if DEBUG_ON
			Debug.LogWarning( "Prefab Infos is null!" );
#endif
			return null;
		}

		for( int i = 0; i < m_PrefabInfos.Count; i++ )
		{
			var info = m_PrefabInfos[i];

			if( prefabName == info.Name )
				return info.Prefab;
		}

		return null;
	}

	/// <summary>
	/// 指定した名前のプレハブを複製して、複製したものを返します。
	/// 該当するプレハブが見つからない場合は、nullを返します。
	/// </summary>
	public GameObject GetInstantiatedObject( string prefabName )
	{
		var origin = GetOriginObject( prefabName );

		if( origin )
		{
			return Instantiate( origin );
		}
		else
		{
			return null;
		}
	}

	#endregion

#if UNITY_EDITOR
	public class PrefabManagerEditor : Editor
	{

		/// <summary>
		/// 保存場所のパス
		/// </summary>
		private static readonly string _path = "Assets/Constant/" + typeof( PrefabManager ).Name + "KeyWord.cs";

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

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUILayout.Space();
			EditorGUILayout.LabelField( "Prefab Keyword Option" );
			EditorGUILayout.LabelField( "Path", _path );

			if( GUILayout.Button( "Generate Prefab Keyword" ) )
			{
				if( !CanGenerate() )
				{
					Debug.LogError( "It cannot generate prefab keyword now!" );
					return;
				}

				var editor = target as PrefabManager;

				if( GenerateScript( editor.PrefabInfos ) )
				{
					EditorUtility.DisplayDialog( "PrefabManager", "作成が完了しました", "OK" );
				}
				else
				{
					EditorUtility.DisplayDialog( "PrefabManager", "作成が失敗しました", "OK" );
				}
			}
		}


		/// <summary>
		/// プレハブ名を定数で管理するクラスを作成できるかどうかを取得します。
		/// </summary>
		private static bool CanGenerate()
		{
			return !EditorApplication.isPlaying && !Application.isPlaying && !EditorApplication.isCompiling;
		}

		/// <summary>
		/// プレハブ名を列挙したスクリプトを生成する。
		/// 生成に成功した場合は true 、失敗した場合は false を返します。
		/// </summary>
		private static bool GenerateScript( List<PrefabInfo> infos )
		{
			try
			{
				var builder = new StringBuilder();
				builder.AppendLine( "/// <summary>" );
				builder.AppendLine( "/// プレハブ名を定数として保持するクラス" );
				builder.AppendLine( "/// </summary>" );
				builder.AppendFormat( "public static class {0}", typeof( PrefabManager ).Name + "KeyWord" ).AppendLine();
				builder.AppendLine( "{" );

				foreach( var info in infos.Select( c => new
			{
				var = RemoveInvalidChars( c.Name )
					      , val = c.Name
				} ) )
				{
					builder.Append( "\t" ).AppendFormat( @"public const string {0} = ""{1}"";", info.var, info.val ).AppendLine();
				}

				builder.AppendLine( "}" );

				var directoryName = Path.GetDirectoryName( _path );

				if( !Directory.Exists( directoryName ) )
				{
					Directory.CreateDirectory( directoryName );
				}

				File.WriteAllText( _path, builder.ToString(), Encoding.UTF8 );
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

#region Definition

[Serializable]
internal class PrefabInfo
{
	[SerializeField]
	private string m_Name;

	[SerializeField]
	private GameObject m_Prefab;

	public string Name
	{
		get
		{
			return m_Name;
		}
	}

	public GameObject Prefab
	{
		get
		{
			return m_Prefab;
		}
	}
}

#endregion