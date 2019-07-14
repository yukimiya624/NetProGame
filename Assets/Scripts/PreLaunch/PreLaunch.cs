using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PreLaunchの処理。
/// Created by Sho Yamagami.
/// </summary>
public class PreLaunch : MonoBehaviour
{
    [SerializeField]
    private string m_NextScene;

    Coroutine m_WaitSeq;

    private void Start()
    {
        if (m_WaitSeq != null)
        {
            return;
        }

        m_WaitSeq = StartCoroutine(WaitTransition());
    }

    private IEnumerator WaitTransition()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(m_NextScene);
    }
}
