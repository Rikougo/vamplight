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

        public NPCType Type => m_type;
        
        private void Awake()
        {
            m_type = GetComponent<AggressiveNPC>() is null ? NPCType.PASSIVE : NPCType.AGRESSIVE;
        }

        private void OnTriggerEnter2D(Collider2D p_collider)
        {
            if (p_collider.CompareTag("Player"))
            {
                Player l_player = p_collider.GetComponent<Player>();
                l_player.AddKillable(this);
            }
        }
        
        private void OnTriggerExit2D(Collider2D p_collider)
        {
            if (p_collider.CompareTag("Player"))
            {
                Player l_player = p_collider.GetComponent<Player>();
                l_player.RemoveKillable(this);
            }
        }

        public void Kill()
        {
            GetComponent<SpriteRenderer>().sprite = m_deathSprite;
            this.enabled = false;
        }
    }
}
