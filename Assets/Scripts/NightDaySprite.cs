using System.Collections;
using System.Collections.Generic;
using Scripts.Managers;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class NightDaySprite : MonoBehaviour
{
    [SerializeField] private Sprite m_daySprite;
    [SerializeField] private Sprite m_nightSprite;

    private SpriteRenderer m_renderer;

    private void Awake()
    {
        m_renderer = GetComponent<SpriteRenderer>();
    }

    public void Start()
    {
        GameDirector l_director = GameObject.FindWithTag("GameController").GetComponent<GameDirector>();
        l_director.OnGameStateChange += OnNewState;
        Sprite l_sprite = l_director.CurrentState == GameDirector.GameState.DAY ? m_daySprite : m_nightSprite;
        m_renderer.sprite = l_sprite;
    }
    
    private void OnNewState(GameDirector.GameState p_newState)
    {
        switch (p_newState)
        {
            case GameDirector.GameState.DAY:
                m_renderer.sprite = m_daySprite;
                break;
            case GameDirector.GameState.NIGHT:
                m_renderer.sprite = m_nightSprite;
                break;
        }
    }
}
