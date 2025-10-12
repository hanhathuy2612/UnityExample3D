using System;
using UnityEngine;
using UnityEngine.AI;

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
            Debug.Log("Enemy Died");
            OnDeath?.Invoke();
            var col = GetComponent<Collider>();
            if (col) col.enabled = false;
            var agent = GetComponent<NavMeshAgent>();
            if (agent) agent.enabled = false;
        }
    }
}