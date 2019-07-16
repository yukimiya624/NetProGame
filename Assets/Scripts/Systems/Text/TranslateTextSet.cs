using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu( menuName = "Tool/Translate TextSet", fileName = "Translate TextSet" )]
public class TranslateTextSet : ScriptableObject
{

	[SerializeField]
	private string m_TextSetKey;

	[SerializeField]
	private TranslateTextInfo[] m_TranslateTextInfos;

	public string TextSetKey
	{
		get
		{
			return m_TextSetKey;
		}
	}

	public TranslateTextInfo[] TranslateTextInfos
	{
		get
		{
			return m_TranslateTextInfos;
		}
	}

	public TranslateTextInfo GetTextInfo( TranslateLanguage targetLanguage, TranslateLanguage defaultLanguage )
	{

		// 該当する言語を取得
		foreach( var info in m_TranslateTextInfos )
		{
			if( info == null )
				continue;

			if( info.Language == targetLanguage )
				return info;
		}

		// もし該当しなければデフォルト言語を取得
		foreach( var info in m_TranslateTextInfos )
		{
			if( info == null )
				continue;

			if( info.Language == defaultLanguage )
				return info;
		}

		// それでも該当しなければ null を返す
		return null;
	}

}

[Serializable]
public class TranslateTextInfo
{
	public TranslateLanguage Language;

	public bool UseDefaultTextSet;

	public TextSet TextSet;
}

[Serializable]
public enum TranslateLanguage
{
	// 英語
	English,

	// 日本語
	Japanese,

	// 中国語(簡体)
	ChineseSimplified,

	// 中国語(繁体)
	ChineseTraditional,

	// フランス語
	French,

	// イタリア語
	Italian,

	// ドイツ語
	German,

	// 韓国語
	Korean,

	// ポルトガル語
	Portuguese,

	// ロシア語
	Russian,

	// スペイン語
	Spanish,

	// ヒンディー語
	Hindi,
}