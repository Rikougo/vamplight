using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxItem : MonoBehaviour
{
    public Transform m_camera;
    public float relativeMoveX = .3f;
    public float relativeMoveY = .1f;

    public float m_leftXStart;

    private float m_xStart;
    
    void Start()
    {
        m_xStart = m_camera.position.x;
    }

    void Update()
    {
        float l_deltaPosition = m_camera.position.x - m_xStart;
        transform.position = new Vector2((m_camera.position.x * relativeMoveX) * 0.2f, transform.position.y);
    }
}