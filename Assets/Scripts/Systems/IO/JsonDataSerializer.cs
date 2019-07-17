using UnityEngine;

/// <summary>
/// Json用データシリアライザ。
/// </summary>
public class JsonDataSerializer<T> : BaseDataSerializer where T : class
{

	private T m_JsonData;

	public T JsonData
	{
		get
		{
			return m_JsonData;
		}
		set
		{
			m_JsonData = value;
		}
	}

	public sealed override string Serialize()
	{
		return JsonUtility.ToJson( m_JsonData );
	}

	public sealed override void Deserialize( string rawData )
	{
		m_JsonData = JsonUtility.FromJson<T>( rawData );
	}
}