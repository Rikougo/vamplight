using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Scripts.Lights
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Light2D))]
    public class AggressiveSpotlight : MonoBehaviour
    {
        private bool m_colliding = false;
        private float m_radius = 1.0f;
        private float m_openAngle = 45.0f;
        [SerializeField] private float m_detectionPrecision = 5.0f;
        [SerializeField] private LayerMask m_playerMask;

        public delegate void OnLightEnterHandler(AggressiveSpotlight p_spotLight);
        public delegate void OnLightExitHandler(AggressiveSpotlight p_spotlight);

        public event OnLightEnterHandler OnLightEnter;
        public event OnLightExitHandler OnLightExit;

        private void Update()
        {
            Light2D l_light = GetComponent<Light2D>();

            m_radius = l_light.pointLightOuterRadius;
            m_openAngle = l_light.pointLightOuterAngle;
        }

        private void LateUpdate()
        {
            Collider2D l_playerCollider = Physics2D.OverlapCircle(transform.position, m_radius, m_playerMask);
            bool l_lastState = m_colliding;

            if (l_playerCollider is not null && l_playerCollider.CompareTag("Player"))
            {
                m_colliding = false;

                float l_halfAngle = m_openAngle * 0.5f;
                
                for (float l_angle = (-l_halfAngle); l_angle < l_halfAngle; l_angle += m_detectionPrecision)
                {
                    float l_radAngle = (l_angle - transform.eulerAngles.z) * Mathf.Deg2Rad;
                    Vector2 l_direction = new Vector2(Mathf.Sin(l_radAngle), Mathf.Cos(l_radAngle));
                    RaycastHit2D l_hit = Physics2D.Raycast(
                        transform.position,
                        l_direction, 
                        float.MaxValue, 
                        m_playerMask);

                    if (l_hit.collider == l_playerCollider)
                    {
                        m_colliding = true;
                        break;
                    }
                }
            }
            else m_colliding = false;

            if (l_lastState != m_colliding)
            {
                if (m_colliding) OnLightEnter?.Invoke(this);
                else OnLightExit?.Invoke(this);
            }
        }

        /// <summary>
        /// Debug purpose only
        /// </summary>
        private void OnDrawGizmos()
        {
            if (UnityEditor.EditorApplication.isPlaying == false)
            {
                Vector3 l_pos = transform.position;
                Gizmos.color = m_colliding ? Color.red : Color.yellow;
                float l_halfAngle = m_openAngle * 0.5f;
                float l_halfRot = transform.eulerAngles.z * 0.5f;
                
                for (float l_angle = (-l_halfAngle); l_angle < l_halfAngle; l_angle += m_detectionPrecision)
                {
                    float l_radAngle = (l_angle - transform.eulerAngles.z) * Mathf.Deg2Rad;
                    Vector2 l_direction = new Vector2(Mathf.Sin(l_radAngle), Mathf.Cos(l_radAngle));
                    Gizmos.DrawLine(l_pos, l_pos + new Vector3(l_direction.x, l_direction.y, 0.0f) * (m_radius));
                }
                
                Gizmos.color = m_colliding ? Color.red : Color.green;
                Gizmos.DrawWireSphere(l_pos, m_radius);
            }
        }
    }
}
