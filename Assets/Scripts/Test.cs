using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    Rigidbody m_Rb;
    private void Start()
    {
        m_Rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        var w = Input.GetAxis("Horizontal");
        var h = Input.GetAxis("Vertical");

        var pos = m_Rb.position;
        pos.x += w;
        pos.z += h;
        m_Rb.MovePosition(pos);
    }
}
