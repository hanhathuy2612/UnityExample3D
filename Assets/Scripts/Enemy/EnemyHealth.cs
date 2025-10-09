using System;
using UnityEngine;

namespace Unity.Enemy
{
    public class EnemyHealth : MonoBehaviour
    {
        public int maxHealth = 50;
        public int currentHealth;
        public event Action OnDeath;

        private void Start()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (currentHealth <= 0) return;
            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }

        private void Die()
        {
            OnDeath?.Invoke();
            // optionally disable collider & navmesh agent here
            var col = GetComponent<Collider>();
            if (col) col.enabled = false;
            var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent) agent.enabled = false;
        }
    }
}