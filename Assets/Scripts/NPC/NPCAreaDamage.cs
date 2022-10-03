using System;
using System.Collections;
using System.Collections.Generic;
using Scripts;
using UnityEngine;

public class NPCAreaDamage : MonoBehaviour
{
    [SerializeField] private float m_damagePerSecond = 20.0f;
    [SerializeField] private float m_tickPerSecond = 2.0f;
    private float m_tickTimer = 0.0f;

    private Player m_player;

    public void Update()
    {
        if (m_player is not null)
        {
            m_tickTimer += Time.deltaTime;

            if (m_tickTimer > (1.0f / m_tickPerSecond))
            {
                m_player.TakeDamage(this.gameObject, m_damagePerSecond * m_tickTimer, false);
                m_tickTimer = 0.0f;
            }
        }
    }
    
    public void OnTriggerEnter2D(Collider2D p_other)
    {
        if (p_other.CompareTag("Player"))
        {
            m_player = p_other.GetComponent<Player>();
        }
    }

    public void OnTriggerExit2D(Collider2D p_other)
    {
        if (p_other.CompareTag("Player"))
        {
            m_player = null;
        }
    }
}
