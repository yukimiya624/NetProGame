using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class SaveDataSerializer : BaseDataSerializer
{

	#region Definition

	[Serializable]
	private class Serialization<T>
	{
		[SerializeField]
		private List<T> m_Target;

		public Serialization() { }

		public Serialization( List<T> target )
		{
			m_Target = target;
		}

		public List<T> ToList()
		{
			return m_Target;
		}
	}

	[Serializable]
	private class Serialization<K, V>
	{
		[SerializeField]
		private List<K> m_Keys;

		[SerializeField]
		private List<V> m_Values;

		private Dictionary<K, V> m_Target;

		public Serialization() { }

		public Serialization( Dictionary<K, V> target )
		{
			m_Target = target;
		}

		public void OnBeforeSerialize()
		{
			m_Keys = new List<K>( m_Target.Keys );
			m_Values = new List<V>( m_Target.Values );
		}

		public void OnAfterDeserialize()
		{
			int count = Mathf.Min( m_Keys.Count, m_Values.Count );
			m_Target = new Dictionary<K, V>( count );
			Enumerable.Range( 0, count ).ToList().ForEach( i => m_Target.Add( m_Keys[i], m_Values[i] ) );
		}

		public Dictionary<K, V> ToDictionary()
		{
			return m_Target;
		}
	}

	#endregion



	#region Field Private

	[SerializeField]
	private Dictionary<string, string> m_SaveDictionary;

	#endregion



	#region Constructor

	public SaveDataSerializer( string folderPath, string fileName )
	{
		FolderPath = folderPath;
		FileName = fileName;
		m_SaveDictionary = new Dictionary<string, string>();
	}

	#endregion



	#region Method Private

	/// <summary>
	/// キーに不正がないかチェックする。
	/// </summary>
	private void KeyCheck( string key )
	{
		if( string.IsNullOrEmpty( key ) )
		{
			throw new ArgumentException( "invalid key!!" );
		}
	}

	#endregion



	#region Method Override

	public override string Serialize()
	{
		var serialDict = new Serialization<string, string>( m_SaveDictionary );
		serialDict.OnBeforeSerialize();
		return JsonUtility.ToJson( serialDict );
	}

	public override void Deserialize( string rawData )
	{
		if( m_SaveDictionary != null )
		{
			var sDict = JsonUtility.FromJson<Serialization<string, string>>( rawData );
			sDict.OnAfterDeserialize();
			m_SaveDictionary = sDict.ToDictionary();
		}
		else
		{
			m_SaveDictionary = new Dictionary<string, string>();
		}
	}

	#endregion



	#region Method Public

	public void SetList<T>( string key, List<T> list )
	{
		KeyCheck( key );
		var serializableList = new Serialization<T>( list );
		string json = JsonUtility.ToJson( serializableList );
		m_SaveDictionary[key] = json;
	}

	public List<T> GetList<T>( string key, List<T> _default )
	{
		KeyCheck( key );

		if( !m_SaveDictionary.ContainsKey( key ) )
			return _default;

		string json = m_SaveDictionary[key];
		Serialization<T> deserializeList = JsonUtility.FromJson<Serialization<T>>( json );
		return deserializeList.ToList();
	}

	public T GetStruct<T>( string key, T _default ) where T : struct
	{
		KeyCheck( key );

		if( !m_SaveDictionary.ContainsKey( key ) )
			return _default;

		string json = m_SaveDictionary[key];
		T obj = JsonUtility.FromJson<T>( json );
		return obj;
	}

	public void SetStruct<T>( string key, T obj ) where T : struct
	{
		KeyCheck( key );
		string json = JsonUtility.ToJson( obj );
		m_SaveDictionary[key] = json;
	}

	public T GetClass<T>( string key, T _default ) where T : class, new()
	{
		KeyCheck( key );

		if( !m_SaveDictionary.ContainsKey( key ) )
			return _default;

		string json = m_SaveDictionary[key];
		T obj = JsonUtility.FromJson<T>( json );
		return obj;
	}

	public void SetClass<T>( string key, T obj ) where T : class, new()
	{
		KeyCheck( key );
		string json = JsonUtility.ToJson( obj );
		m_SaveDictionary[key] = json;
	}

	public void SetString( string key, string value )
	{
		KeyCheck( key );
		m_SaveDictionary[key] = value;
	}

	public string GetString( string key, string _default )
	{
		KeyCheck( key );

		if( !m_SaveDictionary.ContainsKey( key ) )
			return _default;

		return m_SaveDictionary[key];
	}

	public void SetInt( string key, int value )
	{
		KeyCheck( key );
		m_SaveDictionary[key] = value.ToString();
	}

	public int GetInt( string key, int _default )
	{
		KeyCheck( key );

		if( !m_SaveDictionary.ContainsKey( key ) )
			return _default;

		int ret;

		if( !int.TryParse( m_SaveDictionary[key], out ret ) )
			ret = _default;

		return ret;
	}

	public void SetFloat( string key, float value )
	{
		KeyCheck( key );
		m_SaveDictionary[key] = value.ToString();
	}

	public float GetFloat( string key, float _default )
	{
		KeyCheck( key );

		if( !m_SaveDictionary.ContainsKey( key ) )
			return _default;

		float ret;

		if( !float.TryParse( m_SaveDictionary[key], out ret ) )
			ret = _default;

		return ret;
	}

	public void SetBool( string key, bool value )
	{
		KeyCheck( key );
		m_SaveDictionary[key] = value.ToString();
	}

	public bool GetBool( string key, bool _default )
	{
		KeyCheck( key );

		if( !m_SaveDictionary.ContainsKey( key ) )
			return _default;

		bool ret;

		if( !bool.TryParse( m_SaveDictionary[key], out ret ) )
			ret = _default;

		return ret;
	}

	public void Clear()
	{
		m_SaveDictionary.Clear();
	}

	public void Remove( string key )
	{
		KeyCheck( key );

		if( m_SaveDictionary.ContainsKey( key ) )
		{
			m_SaveDictionary.Remove( key );
		}

	}

	public bool ContainsKey( string _key )
	{
		return m_SaveDictionary.ContainsKey( _key );
	}

	public List<string> GetKeys()
	{
		return m_SaveDictionary.Keys.ToList();
	}

	public string GetJsonString( string key )
	{
		KeyCheck( key );

		if( m_SaveDictionary.ContainsKey( key ) )
		{
			return m_SaveDictionary[key];
		}
		else
		{
			return null;
		}
	}

	#endregion

}