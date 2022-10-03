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
    [SerializeField] private float m_fadeDuration = 0.5f;
    private float m_fadeTimer;
    private float m_fromIntensity;
    private float m_targetIntensity;
    
    private void Awake()
    {
        m_light = GetComponent<Light2D>();
    }

    public void Start()
    {
        GameDirector l_director = GameObject.FindWithTag("GameController").GetComponent<GameDirector>();
        l_director.OnGameStateChange += OnNewState;
        m_targetIntensity = l_director.CurrentState == GameDirector.GameState.DAY ? m_dayIntensity : m_nightIntensity;
        m_fromIntensity = 0.0f;
        m_fadeTimer = 0.0f;
    }

    public void Update()
    {
        if (m_fadeTimer < m_fadeDuration)
        {
            m_light.intensity = Mathf.Lerp(m_fromIntensity, m_targetIntensity, m_fadeTimer / m_fadeDuration);
            m_fadeTimer += Time.deltaTime;
        }
    }

    private void OnNewState(GameDirector.GameState p_state)
    {
        switch (p_state)
        {
            case GameDirector.GameState.DAY:
                m_fromIntensity = m_light.intensity;
                m_targetIntensity = m_dayIntensity;
                m_fadeTimer = 0.0f;
                break;
            case GameDirector.GameState.NIGHT:
                m_fromIntensity = m_light.intensity;
                m_targetIntensity = m_nightIntensity;
                m_fadeTimer = 0.0f;
                break;
        }
    }
}
