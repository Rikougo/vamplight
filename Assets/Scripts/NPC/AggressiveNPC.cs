using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggressiveNPC : MonoBehaviour
{
    [SerializeField] private float m_attackRate;
    [SerializeField] private float m_attackDuration;
    private float m_timeBetweenAttack;
    private float m_lastAttackTimer;
    private bool m_attacking;

    [SerializeField] private GameObject m_attackArea;

    private void Awake()
    {
        m_timeBetweenAttack = 1.0f / m_attackRate;
        m_lastAttackTimer = 0.0f;
        m_attacking = false;
    }

    void Update()
    {
        m_lastAttackTimer += Time.deltaTime;

        if (m_lastAttackTimer > m_timeBetweenAttack)
        {
            if (!m_attacking)
            {
                m_attacking = true;
                m_attackArea.SetActive(true);
            }
            
            if (m_lastAttackTimer > m_timeBetweenAttack + m_attackDuration)
            {
                m_lastAttackTimer = 0.0f;
                m_attackArea.SetActive(false);
                m_attacking = false;
            }
        }
    }
}
