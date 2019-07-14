/// <summary>
/// 送受信する構造体のタイプを識別するための構造体。
/// </summary>
[System.Serializable]
public struct TypeData
{
    /// <summary>
    /// Contentに適用するべき構造体の型名
    /// </summary>
    public string Type;

    /// <summary>
    /// データ本体
    /// </summary>
    public string Content;
}
