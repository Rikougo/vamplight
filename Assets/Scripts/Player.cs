using UnityEngine;

namespace Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : MonoBehaviour
    {
        [Header("Player Componnents")]
        private Rigidbody2D m_rigidBody;
        [SerializeField] SpriteRenderer m_playerSprite;

        [Header("Movement Parameters")]
        [SerializeField] private float m_speed = 1.0f;
        [SerializeField] private float m_currentSpeed = 1.0f;
        private float m_direction = 0f;

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

        public float Direction
        {
            get => m_direction;
            set => m_direction = value;
        }
    
        private void Awake()
        {
            m_rigidBody = GetComponent<Rigidbody2D>();
            
            m_currentSpeed = m_speed;
        }
        
        public void Update()
        {
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
                //m_rigidBody.AddForce(Vector2.up * m_jumpForce, ForceMode2D.Impulse);
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


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(m_groundChecker.position, m_groundCheckerRadius);
        }
    }
}
