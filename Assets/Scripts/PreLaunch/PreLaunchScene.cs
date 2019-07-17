using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreLaunchScene : BaseScene
{
	/// <summary>
	/// BaseSceneManagerで定義されている最初に遷移すべきシーンに遷移する。
	/// </summary>
	protected void Start()
	{
        BaseSceneManager.Instance.LoadOnGameStart();
    }
}
