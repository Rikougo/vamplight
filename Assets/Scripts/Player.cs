using System.Numerics;
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
        
        private GameDirector m_director;

        [Header("Player Components")]
        private Collider2D m_collider;
        private Rigidbody2D m_rigidBody;
        [SerializeField] private SpriteRenderer m_playerSprite;
        [SerializeField] private ParticleSystem m_deathParticles;

        [Header("Movement Parameters")]
        [SerializeField] private float m_speed = 1.0f;
        [SerializeField] private float m_currentSpeed = 1.0f;

        [Header("Jump Parameters")]
        [SerializeField] private float m_jumpForce = 10.0f;
        [SerializeField] private float m_currentJumpForce = 10.0f;
        [SerializeField] private float m_fallThreshold = -2f;
        [SerializeField] private float m_jumpEndGravityScale = 4f;
        [SerializeField] private float m_fallGravityScale = 3f;

        [Header("GroundChecker Parameters")]
        [SerializeField] private Transform m_groundChecker;
        [SerializeField] private float m_groundCheckerRadius;
        [SerializeField] private LayerMask m_groundMask;
        private bool m_grounded = false;

        [Header("Shadow Form Parameters")]
        [SerializeField] private float m_sFSpeed = 7.5f;
        [SerializeField]  private float m_sFJumpForce = 12f;
        private bool m_inShadowForm;
        private bool m_canSF;
        [SerializeField] private float m_sFDuration = 5f;

        [Header("Player stats")] 
        [SerializeField] private float m_maxHealth = 100.0f;
        private float m_currentHealth;
        private bool m_isAlive;
        private bool m_isProtected;

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

        public bool CanTakeDamage => m_isAlive && !m_inShadowForm && !m_isProtected;
    
        private void Awake()
        {
            m_collider = GetComponent<CapsuleCollider2D>();
            m_rigidBody = GetComponent<Rigidbody2D>();

            m_currentSpeed = m_speed;
            m_currentHealth = m_maxHealth;
            m_isAlive = true;
            m_isProtected = false;
        }

        private void Start()
        {
            m_director = GameObject.FindWithTag("GameController").GetComponent<GameDirector>();
        }
        
        public void Update()
        {
            if (!m_isAlive) return;
            
            m_grounded = !(Physics2D.OverlapCircle(m_groundChecker.position, m_groundCheckerRadius, m_groundMask) is null);
        
            m_rigidBody.velocity = new Vector2((m_direction * m_currentSpeed), m_rigidBody.velocity.y);
            if(m_direction < 0)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
            else if(m_direction > 0)
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

        public void OnJump()
        {
            if (m_grounded)
            {
                m_rigidBody.velocity = Vector2.up * m_currentJumpForce;
            }
        }

        public void EndJump()
        {
            m_rigidBody.gravityScale = m_jumpEndGravityScale;
        }

        public void OnShadowForm()
        {
            Debug.Log("pressed Special button");
            if(!m_inShadowForm)
            {
                m_inShadowForm = true;
                m_currentSpeed = m_sFSpeed;
                m_currentJumpForce = m_sFJumpForce;
                m_playerSprite.color = new Color(0, 0, 0, 1);
                Debug.Log("Shadow Form " + m_inShadowForm);
            }
            else if (m_inShadowForm)
            {
                m_inShadowForm = false;
                m_currentSpeed = m_speed;
                m_currentJumpForce = m_jumpForce;
                m_playerSprite.color = new Color(1, 1, 1, 1);
                Debug.Log("Shadow Form " + m_inShadowForm);
            }
        }

        public void TakeDamage(GameObject p_from, float p_amount)
        {
            if (!this.CanTakeDamage) return;

            Vector2 l_damageDirection = (p_from.transform.position - transform.position).normalized;
            m_rigidBody.velocity -= l_damageDirection * 0.5f;

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
                p_timer =>
                {
                    m_playerSprite.color = Color.white;
                }
            );
        }
        
        public void Die()
        {
            if (!m_isAlive || m_inShadowForm) return;

            m_isAlive = false;
            
            m_playerSprite.enabled = false;
            m_rigidBody.isKinematic = true;
            m_rigidBody.velocity = Vector2.zero;
            m_collider.enabled = false;

            m_deathParticles.Play();
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(m_groundChecker.position, m_groundCheckerRadius);
        }
    }
}
