using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 指定したパスに対してディレクトリが存在しない場合は、ディレクトリを自動生成します。
/// </summary>
public class SafeDirectoryGenerator
{
	/// <summary>
	/// 指定したパスのディレクトリが存在する場合はtrueを返します。
	/// 指定したパスがファイルのパスであっても、その親のディレクトリで判定します。
	/// </summary>
	public static bool CheckExistDirectoryPath( string path )
	{
		string directoryPath = path.Substring( 0, path.LastIndexOf( '/' ) );
		return Directory.Exists( directoryPath );
	}

	/// <summary>
	/// ディレクトリを生成します。
	/// ただし、存在している場合は生成しません。
	/// </summary>
	public static void GenerateDirectory( string path )
	{
		string directoryPath = path.Substring( 0, path.LastIndexOf( '/' ) );

		if( Directory.Exists( directoryPath ) )
			return;

		Directory.CreateDirectory( directoryPath );
	}
}