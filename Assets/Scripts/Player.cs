using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NPC;
using Scripts.Managers;
using UnityEngine;
using UnityEngine.Events;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;

namespace Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : MonoBehaviour
    {
        public delegate void OnPlayerDeathHandler(Player p_player);

        public event OnPlayerDeathHandler OnPlayerDeath;
        public UnityEvent<float> OnPlayerHealthUpdate;
        public UnityEvent<float> OnPlayerCooldownUpdate;

        private GameDirector m_director;

        [Header("Player Components")] private Collider2D m_collider;
        private Rigidbody2D m_rigidBody;
        [SerializeField] private SpriteRenderer m_playerSprite;
        [SerializeField] private ParticleSystem m_deathParticles;
        [SerializeField] private ParticleSystem m_sfParticles;

        [Header("Movement Parameters")] [SerializeField]
        private float m_speed = 1.0f;

        [SerializeField] private float m_currentSpeed = 1.0f;

        [Header("Jump Parameters")] [SerializeField]
        private float m_jumpForce = 5.0f;

        [SerializeField] private float m_currentJumpForce = 5.0f;
        [SerializeField] private float m_fallThreshold = -2f;
        [SerializeField] private float m_jumpEndGravityScale = 2.5f;
        [SerializeField] private float m_fallGravityScale = 2f;

        [Header("GroundChecker Parameters")] 
        [SerializeField] private Transform m_groundChecker;

        [SerializeField] private float m_groundCheckerRadius;
        [SerializeField] private LayerMask m_groundMask;
        private bool m_grounded;

        [Header("Shadow Form Parameters")] 
        [SerializeField] private float m_sFSpeed = 5.0f;

        [SerializeField] private float m_sFJumpForce = 5f;
        private bool m_inShadowForm;
        private bool m_shadowFormAvailable;
        private int m_shadowFormTimerID;
        [SerializeField] private float m_sFDuration = 1.5f;
        [SerializeField] private float m_shadowFormCooldown = 5.0f;

        [Header("Player stats")] [SerializeField]
        private float m_maxHealth = 100.0f;

        private float m_currentHealth;
        private bool m_isAlive;
        private bool m_isProtected;
        private bool m_canMove;

        [Header("Killable visualisation")] [SerializeField]
        private Material m_outlineMaterial;

        [SerializeField] private Material m_defaultMaterial;

        private List<KillableNpc> m_killableInRange;

        private float m_direction = 0f;

        public float Direction
        {
            get => m_direction;
            set => m_direction = value;
        }

        public float Health
        {
            get => m_currentHealth;
        }

        private bool CanTakeDamage => m_isAlive && !m_inShadowForm && !m_isProtected;

        private void Awake()
        {
            m_collider = GetComponent<BoxCollider2D>();
            m_rigidBody = GetComponent<Rigidbody2D>();
            m_killableInRange = new List<KillableNpc>();

            m_currentSpeed = m_speed;
            m_currentJumpForce = m_jumpForce;
            m_currentHealth = m_maxHealth;
            m_grounded = false;
            m_isAlive = true;
            m_isProtected = false;
            m_canMove = true;

            m_inShadowForm = false;
            m_shadowFormAvailable = true;
            m_shadowFormTimerID = -1;
        }

        private void Start()
        {
            m_director = GameObject.FindWithTag("GameController").GetComponent<GameDirector>();
        }

        public void Update()
        {
            if (!m_isAlive) return;

            m_grounded = !(Physics2D.OverlapCircle(m_groundChecker.position, m_groundCheckerRadius, m_groundMask) is null);

            if (m_canMove) m_rigidBody.velocity = new Vector2((m_direction * m_currentSpeed), m_rigidBody.velocity.y);
            transform.rotation = Quaternion.Euler(0f, m_direction < 0 ? 180f : 0.0f, 0f);
            if (m_direction < 0)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
            else if (m_direction > 0)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }

            if (m_grounded)
            {
                m_rigidBody.gravityScale = 1f;
            }
            else if (!m_grounded && m_rigidBody.velocity.y < m_fallThreshold)
            {
                m_rigidBody.gravityScale = m_fallGravityScale;
            }
        }
        /// <summary>
        /// A basidc videogame jump.
        /// </summary>
        /// <param name="context"></param>
        public void OnJump()
        {
            if (m_grounded)
            {
                m_rigidBody.velocity = Vector2.up * m_currentJumpForce;
            }
        }
        /// <summary>
        /// Makes it so that when the player releases the jump key, they fall a little bit faster to emulate a better jump physic.
        /// </summary>
        /// <param name="context"></param>
        public void EndJump()
        {
            m_rigidBody.gravityScale = m_jumpEndGravityScale;
        }

        /// <summary>
        /// Activates the shadow form which protects the player from light and allows them to move faster.
        /// </summary>
        /// <param name="context"></param>
        public void OnShadowForm()
        {
            if (m_shadowFormAvailable)
            {
                if (m_shadowFormTimerID != -1) m_director.CancelTimer(m_shadowFormTimerID);

                m_shadowFormAvailable = false;

                SetShadowForm(true);

                // Delay to end
                m_director.AddDelayedAction(m_sFDuration, (_) =>
                {
                    SetShadowForm(false);

                    // Start cooldown
                    m_shadowFormTimerID = m_director.AddTimer(m_shadowFormCooldown, 
                        (p_timer, _) => this.OnPlayerCooldownUpdate?.Invoke(p_timer.Progress), 
                        (_) =>
                            {
                                this.OnPlayerCooldownUpdate?.Invoke(1.0f);
                                m_shadowFormAvailable = true;
                            });
                });
            }
        }

        private void SetShadowForm(bool p_active)
        {
            m_inShadowForm = p_active;
            m_currentSpeed = p_active ? m_sFSpeed : m_speed;
            m_currentJumpForce = p_active ? m_sFJumpForce : m_jumpForce;
            m_playerSprite.color = p_active ? Color.black : Color.white;
            if (p_active) m_sfParticles.Play();
            else m_sfParticles.Stop();
        }

        public void TakeDamage(GameObject p_from, float p_amount)
        {
            if (!this.CanTakeDamage) return;

            Vector2 l_knockbackDirection = ((transform.position.x > p_from.transform.position.x ? Vector2.right : Vector2.left) + Vector2.up).normalized;
            m_rigidBody.velocity = (l_knockbackDirection) * 5.0f;
            m_canMove = false;

            m_currentHealth -= p_amount;
            this.OnPlayerHealthUpdate?.Invoke(m_currentHealth / m_maxHealth);
            if (m_currentHealth <= 0.0f)
            {
                this.Die();
                return;
            }

            
            m_director.AddTimer(
                0.75f,
                (p_timer, p_deltaTime) =>
                {
                    float l_progress = p_timer.Progress * 6.0f * Mathf.PI;
                    float l_redBlueValue = Mathf.Sin(l_progress) * 0.5f + 0.5f;
                    m_playerSprite.color = new Color(1.0f, l_redBlueValue, l_redBlueValue);
                },
                p_timer => { 
                    m_playerSprite.color = Color.white;
                    m_canMove = true;
                }
            );
        }

        public void Die()
        {
            if (!m_isAlive || m_inShadowForm) return;

            m_isAlive = false;
            this.OnPlayerDeath?.Invoke(this);

            m_playerSprite.gameObject.SetActive(false);
            m_rigidBody.isKinematic = true;
            m_rigidBody.velocity = Vector2.zero;
            m_collider.enabled = false;

            m_deathParticles.Play();
        }

        public void AddKillable(KillableNpc p_npc)
        {
            m_killableInRange.Add(p_npc);

            if (m_killableInRange.Count == 1)
            {
                KillableNpc l_target = m_killableInRange.First();
                SpriteRenderer l_targetRenderer = l_target.GetComponent<SpriteRenderer>();

                if (l_targetRenderer is not null)
                {
                    l_targetRenderer.material = m_outlineMaterial;
                }
            }
        }

        public void RemoveKillable(KillableNpc p_npc)
        {
            if (m_killableInRange.Find((p_value) => p_value == p_npc))
            {
                SpriteRenderer l_npcRenderer = p_npc.GetComponent<SpriteRenderer>();
                if (l_npcRenderer is not null)
                {
                    l_npcRenderer.material = m_defaultMaterial;
                }

                m_killableInRange.Remove(p_npc);

                if (m_killableInRange.Count == 1)
                {
                    KillableNpc l_target = m_killableInRange.First();
                    SpriteRenderer l_targetRenderer = l_target.GetComponent<SpriteRenderer>();

                    if (l_targetRenderer is not null)
                    {
                        l_targetRenderer.material = m_outlineMaterial;
                    }
                }
            }
        }

        public void KillSelected()
        {
            if (m_killableInRange.Count >= 1)
            {
                KillableNpc l_target = m_killableInRange.First();
                transform.position = l_target.transform.position;
                m_deathParticles.Play();
                m_currentHealth += 20.0f;
                this.OnPlayerHealthUpdate?.Invoke(m_currentHealth / m_maxHealth);
                Destroy(l_target.gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(m_groundChecker.position, m_groundCheckerRadius);
        }
    }
}