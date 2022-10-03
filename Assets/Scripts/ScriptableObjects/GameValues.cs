using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "GameValues", menuName = "Vamplight/GameValues", order = 1)]
    public class GameValues : ScriptableObject
    {
        public LayerMask GroundMask;
        public LayerMask PlayerMask;
    }
}
