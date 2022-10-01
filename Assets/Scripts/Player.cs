using UnityEngine;

namespace Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : MonoBehaviour
    {
        [SerializeField] private float m_speed = 1.0f;
        [SerializeField] private float m_jumpForce = 10.0f;
        [SerializeField] private Transform m_groundChecker;
        [SerializeField] private LayerMask m_groundMask;
        private Rigidbody2D m_rigidBody;

        private bool m_grounded = false;
        private float m_direction = 0f;
        public float Direction
        {
            get => m_direction;
            set => m_direction = value;
        }
    
        private void Awake()
        {
            m_rigidBody = GetComponent<Rigidbody2D>();
        }

        public void Update()
        {
            m_grounded = !(Physics2D.OverlapCircle(m_groundChecker.position, 0.1f, m_groundMask) is null);
        
            m_rigidBody.velocity = new Vector2((m_direction * m_speed), m_rigidBody.velocity.y);
        }

        public void OnJump()
        {
            if (m_grounded)
            {
                m_rigidBody.AddForce(Vector2.up * m_jumpForce, ForceMode2D.Impulse); 
            }
        }
    }
}
