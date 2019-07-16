using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

/// <summary>
/// 動的引数パラメータ
/// </summary>
public class ArgumentParamSet
{
	public Dictionary<string, int> IntParam;
	public Dictionary<string, float> FloatParam;
	public Dictionary<string, bool> BoolParam;
    public Dictionary<string, string> StringParam;
	public Dictionary<string, Vector2> V2Param;
	public Dictionary<string, Vector3> V3Param;

	public void ApplyIntParam( string key, ref int apply )
	{
		if( IntParam != null && IntParam.ContainsKey( key ) )
		{
			apply = IntParam[key];
		}
	}

	public void ApplyFloatParam( string key, ref float apply )
	{
		if( FloatParam != null && FloatParam.ContainsKey( key ) )
		{
			apply = FloatParam[key];
		}
	}

	public void ApplyBoolParam( string key, ref bool apply )
	{
		if( BoolParam != null && BoolParam.ContainsKey( key ) )
		{
			apply = BoolParam[key];
		}
    }

    public void ApplyStringParam(string key, ref string apply)
    {
        if (StringParam != null && StringParam.ContainsKey(key))
        {
            apply = StringParam[key];
        }
    }

    public void ApplyV2Param( string key, ref Vector2 apply )
	{
		if( V2Param != null && V2Param.ContainsKey( key ) )
		{
			apply = V2Param[key];
		}
	}

	public void ApplyV3Param( string key, ref Vector3 apply )
	{
		if( V3Param != null && V3Param.ContainsKey( key ) )
		{
			apply = V3Param[key];
		}
	}

	public void DumpParam()
	{
		var builder = new StringBuilder();

		if( IntParam != null )
		{
			builder.Append( "Int Param\n" );

			foreach( var p in IntParam )
			{
				builder.AppendFormat( "{0} : {1}\n", p.Key, p.Value );
			}

			builder.Append( "\n" );
		}

		if( FloatParam != null )
		{
			builder.Append( "Float Param\n" );

			foreach( var p in FloatParam )
			{
				builder.AppendFormat( "{0} : {1}\n", p.Key, p.Value );
			}

			builder.Append( "\n" );
		}

		if( BoolParam != null )
		{
			builder.Append( "Bool Param\n" );

			foreach( var p in BoolParam )
			{
				builder.AppendFormat( "{0} : {1}\n", p.Key, p.Value );
			}

			builder.Append( "\n" );
		}

        if (StringParam != null)
        {
            builder.Append("String Param\n");

            foreach (var p in StringParam)
            {
                builder.AppendFormat("{0} : {1}\n", p.Key, p.Value);
            }

            builder.Append("\n");
        }

        if ( V2Param != null )
		{
			builder.Append( "Vector2 Param\n" );

			foreach( var p in V2Param )
			{
				builder.AppendFormat( "{0} : {1}\n", p.Key, p.Value );
			}

			builder.Append( "\n" );
		}

		if( V3Param != null )
		{
			builder.Append( "Vector3 Param\n" );

			foreach( var p in V3Param )
			{
				builder.AppendFormat( "{0} : {1}\n", p.Key, p.Value );
			}

			builder.Append( "\n" );
		}

		Debug.Log( builder );
	}
}
