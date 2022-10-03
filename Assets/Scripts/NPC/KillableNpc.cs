using System;
using Scripts;
using UnityEngine;

namespace NPC
{
    public class KillableNpc : MonoBehaviour
    {
        public enum NPCType {
            PASSIVE,
            AGRESSIVE
        };
    
        [SerializeField] private Sprite m_deathSprite;
        private NPCType m_type;
        private Player m_player;
        private bool m_alive;

        public NPCType Type => m_type;
        [SerializeField] private float m_regenAmount = 20.0f;

        public float RegenAmount => m_regenAmount;
        
        private void Awake()
        {
            m_alive = true;
            m_type = GetComponent<AggressiveNPC>() is null ? NPCType.PASSIVE : NPCType.AGRESSIVE;
        }

        private void OnTriggerEnter2D(Collider2D p_collider)
        {
            if (!m_alive) return;

            if (p_collider.CompareTag("Player"))
            {
                Player l_player = p_collider.GetComponent<Player>();
                l_player.AddKillable(this);
                m_player = l_player;
            }
        }
        
        private void OnTriggerExit2D(Collider2D p_collider)
        {
            if (!m_alive) return;

            if (p_collider.CompareTag("Player"))
            {
                Player l_player = p_collider.GetComponent<Player>();
                l_player.RemoveKillable(this);
                m_player = null;
            }
        }

        public void StartKill()
        {
            m_alive = false;
        }

        public void Kill()
        {
            GetComponent<SpriteRenderer>().sprite = m_deathSprite;
            if (m_player is not null) m_player.RemoveKillable(this);
        }
    }
}
