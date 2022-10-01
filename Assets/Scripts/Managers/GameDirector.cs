using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameDirector : MonoBehaviour
{
    private PlayerInput m_input;
    public PlayerInput Input => m_input;

    [SerializeField] private Player m_player;

    private void Awake()
    {
        m_input = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        if (m_input == null)
        {
            Debug.LogError("Player input not set on GameDirector script.");
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        m_input.actions["Move"].performed += (p_ctx) => SendMoveDirection(p_ctx.ReadValue<float>());
        m_input.actions["Move"].canceled += (_) => SendMoveDirection(0.0f);

        m_input.actions["Jump"].performed += (_) => m_player.OnJump();
    }

    private void SendMoveDirection(float p_direction)
    {
        m_player.Direction = p_direction;
    }
}
