using System;
using System.Collections;
using System.Collections.Generic;
using Scripts;
using UnityEngine;

public class DangerousCollider : MonoBehaviour
{
    public float damageAmount = 10.0f;
    
    private void OnCollisionEnter2D(Collision2D p_collider)
    {
        if (p_collider.gameObject.CompareTag("Player"))
        {
            p_collider.gameObject.GetComponent<Player>().TakeDamage(this.gameObject, damageAmount);
        }
    }
}
