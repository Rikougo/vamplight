using System;
using System.Collections;
using System.Collections.Generic;
using Scripts.Managers;
using UnityEngine;

public class WinCondition : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D p_collider)
    {
        if (p_collider.CompareTag("Player"))
        {
            GameObject.FindWithTag("GameController").GetComponent<GameDirector>().Win();
        } 
    }
}
