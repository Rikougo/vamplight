using System.Collections.Generic;
using System.Linq;
using Scripts.Managers.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.Managers
{
    public class GameDirector : MonoBehaviour
    {
        private Dictionary<int, TimerHolder> m_timers;
        private PlayerInput m_input;
        public PlayerInput Input => m_input;

        [SerializeField] private Player m_player;

        private void Awake()
        {
            m_timers = new Dictionary<int, TimerHolder>();
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

        private void Update()
        {
            TickTimers();
        }

        private void SendMoveDirection(float p_direction)
        {
            m_player.Direction = p_direction;
        }
        
        #region TIMERS
        private void TickTimers()
        {
            List<int> l_toDelete = new List<int>();
            List<int> l_keys = m_timers.Keys.ToList();
            for(int l_index = 0; l_index < l_keys.Count; l_index++)
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

        public void EndTimer(int p_id)
        {
            if (HasTimer(p_id))
            {
                TimerHolder l_timer = m_timers[p_id];
                l_timer.End();
            }
        }
        
        public int AddTimer(TimerHolder p_timer)
        {
            if (p_timer.Started || p_timer.Ended)
            {
                Debug.LogWarning("Registering an already started or ended Timer.");
            }
            
            p_timer.Start();
            int l_id = Mathf.RoundToInt(Time.time * 100);
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
    }
}
