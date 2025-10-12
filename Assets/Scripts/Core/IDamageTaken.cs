using UnityEngine;

namespace Unity.Core
{
    public abstract class IDamageTaken : MonoBehaviour
    {
        [SerializeField] protected HealthBar healthBar;
        public abstract void TakeDamage(int amount);
    }
}