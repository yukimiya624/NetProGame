using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

/// <summary>
/// 動的引数パラメータに変換するクラス。
/// </summary>
public class ArgumentParamSetTranslator
{
    #region TranslateFromString

    /// <summary>
    /// 指定された形式の文字列から動的引数パラメータに変換する。
    /// </summary>
    public static ArgumentParamSet TranslateFromString(string paramData)
    {
        var paramSet = new ArgumentParamSet();

        if (paramData == null)
        {
            return null;
        }

        string[] paramDataArray = paramData.Split(';');

        foreach (var data in paramDataArray)
        {
            string[] pArray = data.Trim().Split(':');

            if (pArray.Length != 3)
            {
                continue;
            }

            string type = pArray[0].Trim();
            string name = pArray[1].Trim();
            string value = pArray[2].Trim();

            switch (type)
            {
                case "I":
                    AddIntParam(paramSet, name, value);
                    break;

                case "F":
                    AddFloatParam(paramSet, name, value);
                    break;

                case "B":
                    AddBoolParam(paramSet, name, value);
                    break;

                case "S":
                    AddStringParam(paramSet, name, value);
                    break;

                case "V2":
                    AddVector2Param(paramSet, name, value);
                    break;

                case "V3":
                    AddVector3Param(paramSet, name, value);
                    break;
            }
        }

        return paramSet;
    }

    private static void AddIntParam(ArgumentParamSet set, string name, string value)
    {
        int intR;

        if (int.TryParse(value, out intR))
        {
            if (set.IntParam == null)
            {
                set.IntParam = new Dictionary<string, int>();
            }

            set.IntParam.Add(name, intR);
        }
    }

    private static void AddFloatParam(ArgumentParamSet set, string name, string value)
    {
        float floatR;

        if (float.TryParse(value, out floatR))
        {
            if (set.FloatParam == null)
            {
                set.FloatParam = new Dictionary<string, float>();
            }

            set.FloatParam.Add(name, floatR);
        }
    }

    private static void AddBoolParam(ArgumentParamSet set, string name, string value)
    {
        bool boolR;

        if (bool.TryParse(value, out boolR))
        {
            if (set.BoolParam == null)
            {
                set.BoolParam = new Dictionary<string, bool>();
            }

            set.BoolParam.Add(name, boolR);
        }
    }

    private static void AddStringParam(ArgumentParamSet set, string name, string value)
    {
        if (set.StringParam == null)
        {
            set.StringParam = new Dictionary<string, string>();
        }

        set.StringParam.Add(name, value);
    }

    private static void AddVector2Param(ArgumentParamSet set, string name, string value)
    {
        // ()内の文字列を抽出
        Match match = Regex.Match(value, "\\((.+)\\)");
        string inValue = match.Result("$1");

        if (inValue == null)
        {
            return;
        }

        string[] pArray = inValue.Split(',');
        float x;
        float y;

        float.TryParse(pArray[0].Trim(), out x);
        float.TryParse(pArray[1].Trim(), out y);

        if (set.V2Param == null)
        {
            set.V2Param = new Dictionary<string, Vector2>();
        }

        set.V2Param.Add(name, new Vector2(x, y));
    }

    private static void AddVector3Param(ArgumentParamSet set, string name, string value)
    {
        // ()内の文字列を抽出
        Match match = Regex.Match(value, "\\((.+)\\)");
        string inValue = match.Result("$1");

        if (inValue == null)
        {
            return;
        }

        string[] pArray = inValue.Split(',');
        float x;
        float y;
        float z;

        float.TryParse(pArray[0].Trim(), out x);
        float.TryParse(pArray[1].Trim(), out y);
        float.TryParse(pArray[2].Trim(), out z);

        if (set.V3Param == null)
        {
            set.V3Param = new Dictionary<string, Vector3>();
        }

        set.V3Param.Add(name, new Vector3(x, y, z));
    }

    #endregion


    /// <summary>
    /// インスペクタで指定するタイプの動的引数から動的引数パラメータに変換する。
    /// </summary>
    /// <param name="variables"></param>
    /// <returns></returns>
    public static ArgumentParamSet TranslateFromArgumentVariables(ArgumentVariable[] variables)
    {
        var paramSet = new ArgumentParamSet();

        if (variables == null)
        {
            return paramSet;
        }

        foreach (var variable in variables)
        {
            switch (variable.Type)
            {
                case E_ARGUMENT_VARIABLE_TYPE.INT:
                    if (paramSet.IntParam == null)
                    {
                        paramSet.IntParam = new Dictionary<string, int>();
                    }

                    paramSet.IntParam.Add(variable.Name, variable.IntValue);
                    break;
                case E_ARGUMENT_VARIABLE_TYPE.FLOAT:
                    if (paramSet.FloatParam == null)
                    {
                        paramSet.FloatParam = new Dictionary<string, float>();
                    }

                    paramSet.FloatParam.Add(variable.Name, variable.FloatValue);
                    break;
                case E_ARGUMENT_VARIABLE_TYPE.BOOL:
                    if (paramSet.BoolParam == null)
                    {
                        paramSet.BoolParam = new Dictionary<string, bool>();
                    }

                    paramSet.BoolParam.Add(variable.Name, variable.BoolValue);
                    break;
                case E_ARGUMENT_VARIABLE_TYPE.STRING:
                    if (paramSet.StringParam == null)
                    {
                        paramSet.StringParam = new Dictionary<string, string>();
                    }

                    paramSet.StringParam.Add(variable.Name, variable.StringValue);
                    break;
                case E_ARGUMENT_VARIABLE_TYPE.VECTOR2:
                    if (paramSet.V2Param == null)
                    {
                        paramSet.V2Param = new Dictionary<string, Vector2>();
                    }

                    paramSet.V2Param.Add(variable.Name, variable.Vector2Value);
                    break;
                case E_ARGUMENT_VARIABLE_TYPE.VECTOR3:
                    if (paramSet.V3Param == null)
                    {
                        paramSet.V3Param = new Dictionary<string, Vector3>();
                    }

                    paramSet.V3Param.Add(variable.Name, variable.Vector3Value);
                    break;
            }
        }

        return paramSet;
    }
}
