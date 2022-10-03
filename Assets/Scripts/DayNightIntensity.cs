using System.Collections;
using System.Collections.Generic;
using Scripts.Managers;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class DayNightIntensity : MonoBehaviour
{
    private Light2D m_light;

    [SerializeField] private float m_dayIntensity;
    [SerializeField] private float m_nightIntensity;

    private void Awake()
    {
        m_light = GetComponent<Light2D>();
    }

    public void Start()
    {
        GameObject.FindWithTag("GameController").GetComponent<GameDirector>().OnGameStateChange += OnNewState;
    }

    private void OnNewState(GameDirector.GameState p_state)
    {
        switch (p_state)
        {
            case GameDirector.GameState.DAY:
                m_light.intensity = m_dayIntensity;
                break;
            case GameDirector.GameState.NIGHT:
                m_light.intensity = m_nightIntensity;
                break;
        }
    }
}
