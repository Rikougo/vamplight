using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using ScriptableObjects;
using Scripts.Managers.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Scripts.Managers
{
    public class GameDirector : MonoBehaviour
    {
        public enum GameState
        {
            DAY,
            NIGHT
        }

        private static GameValues s_valuesInstance;

        public static GameValues Values
        {
            get
            {
                if (!s_valuesInstance)
                {
                    s_valuesInstance = Resources.Load<GameValues>("VamplightSettings");
                }

                return s_valuesInstance;
            }
        }

        public static LayerMask GroundLayerMask;

        private Dictionary<int, TimerHolder> m_timers;
        private PlayerInput m_input;
        public PlayerInput Input => m_input;

        [Header("Scene entities")] [SerializeField]
        private Volume m_globalVolume;

        [SerializeField] private Player m_player;
        [SerializeField] private CinemachineBrain m_cameraBrain;

        [Header("Configuration assets")] [SerializeField]
        private VolumeProfile m_defaultVolumeProfile;

        [SerializeField] private VolumeProfile m_deathVolumeProfile;

        [Header("Day/Night settings")] [SerializeField]
        private float m_dayNightDuration = 10.0f;

        [SerializeField] private float m_dayNightAcceleration = 1.0f;
        private float m_dayNightTimer;
        private GameState m_currentState;

        [Header("UI Misc")] 
        [SerializeField] private TextMeshProUGUI m_stateText;

        public GameState CurrentState => m_currentState;

        private void Awake()
        {
            m_timers = new Dictionary<int, TimerHolder>();
            m_input = GetComponent<PlayerInput>();

            m_dayNightTimer = 0.0f;
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

            m_input.actions["Move"].performed += OnMoveAction;
            m_input.actions["Move"].canceled += OnMoveAction;

            m_input.actions["Jump"].performed += OnJumpActionStart;
            m_input.actions["Jump"].canceled += OnJumpActionEnd;

            m_input.actions["Special"].started += OnSpecialAction;
            m_input.actions["Attack"].started += OnAttackAction;

            m_player.OnPlayerDeath += this.OnPlayerDeath;
        }

        private void OnDestroy()
        {
            m_input.actions["Move"].performed -= OnMoveAction;
            m_input.actions["Move"].canceled -= OnMoveAction;

            m_input.actions["Jump"].performed -= OnJumpActionStart;
            m_input.actions["Jump"].canceled -= OnJumpActionEnd;

            m_input.actions["Special"].started -= OnSpecialAction;
            m_input.actions["Attack"].started -= OnAttackAction;

            m_player.OnPlayerDeath -= this.OnPlayerDeath;
        }

        #region CALLBACK_WRAPPER

        private void OnMoveAction(InputAction.CallbackContext p_ctx)
        {
            this.SendMoveDirection(p_ctx.ReadValue<float>());
        }

        private void OnJumpActionStart(InputAction.CallbackContext p_ctx)
        {
            this.m_player.OnJump();
        }

        private void OnJumpActionEnd(InputAction.CallbackContext p_ctx)
        {
            this.m_player.EndJump();
        }

        private void OnSpecialAction(InputAction.CallbackContext p_ctx)
        {
            this.m_player.OnShadowForm();
        }

        private void OnAttackAction(InputAction.CallbackContext p_ctx)
        {
            this.m_player.KillSelected();
        }

        #endregion

        private void Update()
        {
            switch (m_currentState)
            {
                case GameState.DAY:
                    // DayUpdate();
                    break;
                case GameState.NIGHT:
                    NightUpdate();
                    break;
            }

            UpdateDayNightCycle();
            TickTimers();
        }

        private void DayUpdate()
        {
            RaycastHit2D l_hit = Physics2D.Raycast(
                m_player.transform.position,
                Vector2.up,
                float.MaxValue,
                Values.GroundMask);

            if (l_hit.collider is null)
                m_player.Die();
        }

        private void NightUpdate()
        {
        }

        private void UpdateDayNightCycle()
        {
            m_dayNightTimer += Time.deltaTime * m_dayNightAcceleration;

            if (m_dayNightTimer > m_dayNightDuration)
            {
                m_dayNightTimer = 0.0f;
                m_currentState = m_currentState == GameState.DAY ? GameState.NIGHT : GameState.DAY;
                this.OnGameStateChange?.Invoke(m_currentState);
            }

            m_stateText.text = $"{m_currentState} [{m_dayNightTimer:#.##}]";
        }

        private void SendMoveDirection(float p_direction)
        {
            m_player.Direction = p_direction;
        }

        private void OnPlayerDeath(Player p_player)
        {
            m_globalVolume.profile = m_deathVolumeProfile;

            const float targetAmplitudeGain = 2.5f;
            CinemachineVirtualCamera l_virtualCam = m_cameraBrain.ActiveVirtualCamera as CinemachineVirtualCamera;

            if (l_virtualCam is null)
            {
                AfterPlayerDeathAnimation();
                return;
            }

            var l_noise = l_virtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            l_noise.m_AmplitudeGain = targetAmplitudeGain;

            this.AddTimer(0.75f,
                (TimerHolder p_timer, float p_deltaTime) =>
                {
                    float l_invertProgress = 1.0f - p_timer.Progress;
                    float l_coef = Math.Abs(l_invertProgress - 1.0f) < 0.001f
                        ? 1.0f
                        : 1 - Mathf.Pow(2.0f, -10.0f * l_invertProgress);
                    l_noise.m_AmplitudeGain = targetAmplitudeGain * l_coef;
                },
                (_) =>
                {
                    l_noise.m_AmplitudeGain = 0.0f;
                    AfterPlayerDeathAnimation();
                });
        }

        // TODO ADD ON DEATH BEHAVIOUR
        // SUCH AS LOADING LAST CHECKPOINT OR RELOADING SCENE
        private void AfterPlayerDeathAnimation()
        {
            SceneManager.LoadScene(1, LoadSceneMode.Single);
        }

        #region TIMERS

        private void TickTimers()
        {
            List<int> l_toDelete = new List<int>();
            List<int> l_keys = m_timers.Keys.ToList();
            for (int l_index = 0; l_index < l_keys.Count; l_index++)
            {
                int l_key = l_keys[l_index];
                TimerHolder l_timer = m_timers[l_key];
                if (!l_timer.UpdateTimer(Time.deltaTime))
                {
                    l_toDelete.Add(l_key);
                }
            }

            for (int l_index = 0; l_index < l_toDelete.Count; l_index++)
            {
                m_timers.Remove(l_toDelete[l_index]);
            }
        }

        public bool HasTimer(int p_id)
        {
            return m_timers.ContainsKey(p_id);
        }

        public bool EndTimer(int p_id)
        {
            if (HasTimer(p_id))
            {
                TimerHolder l_timer = m_timers[p_id];
                l_timer.End();

                return true;
            }

            return false;
        }

        public bool CancelTimer(int p_id)
        {
            if (HasTimer(p_id))
            {
                TimerHolder l_timer = m_timers[p_id];
                l_timer.Cancel();

                return true;
            }

            return false;
        }

        public int AddTimer(TimerHolder p_timer)
        {
            if (p_timer.Started || p_timer.Ended)
            {
                // Debug.LogWarning("Registering an already started or ended Timer.");
                return -1;
            }

            p_timer.Start();
            int l_id = Mathf.RoundToInt(DateTime.Now.Millisecond + 1 << Random.Range(5, 8));
            m_timers.Add(l_id, p_timer);

            return l_id;
        }

        public int AddTimer(
            float p_duration,
            TimerHolder.OnUpdateHandler p_updateFunc,
            TimerHolder.OnEndHandler p_endFunc)
        {
            TimerHolder l_timer = new TimerHolder() { Duration = p_duration };
            l_timer.OnUpdate += p_updateFunc;
            l_timer.OnEnd += p_endFunc;

            return this.AddTimer(l_timer);
        }

        public int AddDelayedAction(float p_delay, TimerHolder.OnEndHandler p_function)
        {
            if (p_delay <= 0.0f) return -1;

            TimerHolder l_timer = new TimerHolder() { Duration = p_delay };
            l_timer.OnEnd += p_function;

            return AddTimer(l_timer);
        }

        #endregion

        public delegate void OnGameStateChangeHandler(GameState p_newState);
        public event OnGameStateChangeHandler OnGameStateChange;
    }
}