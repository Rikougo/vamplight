using System;
using System.Collections;
using System.Collections.Generic;
using Scripts;
using UnityEngine;

public class PikeTest : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D p_collider)
    {
        if (p_collider.CompareTag("Player"))
        {
            p_collider.GetComponent<Player>().TakeDamage(this.gameObject, 10);
        }
    }
}
