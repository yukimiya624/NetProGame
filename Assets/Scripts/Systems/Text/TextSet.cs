using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

/// <summary>
/// テキストのフォーマットを指定するためのクラス。
/// </summary>
[Serializable]
public class TextSet
{
	[Multiline]
	public string Text;

	public bool IsUseNewLine;

	public bool IsRichText;

	public Font Font;

	public FontStyle FontStyle;

	public bool IsUseBestFit;

	public int FontSize;

	public int MinFontSize;

	public int MaxFontSize;

	public TextAnchor Alignment;

	public bool IsAlignmentByGeometry;

	public Color Color;

	public void SetText( Text text )
	{
		SetText( text, IsUseNewLine );
	}

	public void SetText( Text text, bool useNewLine )
	{
		var newLine = Environment.NewLine;

		if( useNewLine )
			text.text = string.Format( "{0}{1}{0}", newLine, Text );
		else
			text.text = Text;

		text.supportRichText = IsRichText;
		text.font = Font;
		text.fontStyle = FontStyle;

		text.resizeTextForBestFit = IsUseBestFit;
		text.fontSize = FontSize;
		text.resizeTextMinSize = MinFontSize;
		text.resizeTextMaxSize = MaxFontSize;

		text.alignment = Alignment;
		text.alignByGeometry = IsAlignmentByGeometry;
		text.color = Color;
	}
}