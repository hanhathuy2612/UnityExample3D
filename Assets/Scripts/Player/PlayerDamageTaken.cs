using Unity.Core;
using UnityEngine;

namespace Unity.Player
{
    public class PlayerDamageTaken : MonoBehaviour
    {
        [SerializeField] private HealthBarWorld healthBar;

        public class DamageTakenData
        {
            public int amount;
            public GameObject attacker;
        }

        public void TakeDamage(DamageTakenData data)
        {
            Debug.Log($"Player took {data.amount} damage from {data.attacker.name}");
            healthBar.SetHealth(data.amount);
        }
    }
}