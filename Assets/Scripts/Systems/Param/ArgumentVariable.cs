using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 汎用引数
/// </summary>
[System.Serializable]
public struct ArgumentVariable
{
    public E_ARGUMENT_VARIABLE_TYPE Type;
    public string Name;
    public int IntValue;
    public float FloatValue;
    public bool BoolValue;
    public string StringValue;
    public Vector2 Vector2Value;
    public Vector3 Vector3Value;
}
