using System;
using Scripts;
using UnityEngine;

namespace NPC
{
    public class KillableNpc : MonoBehaviour
    {
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
    }
}
