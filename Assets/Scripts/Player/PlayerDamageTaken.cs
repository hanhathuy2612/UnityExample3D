using Unity.Core;
using UnityEngine;

namespace Unity.Player
{
    public class PlayerDamageTaken : MonoBehaviour
    {
        [SerializeField] private HealthBar healthBar;

        public class DamageTakenData
        {
            public int amount;
            public GameObject attacker;
        }

        public void TakeDamage(DamageTakenData data)
        {
            healthBar.SetHealth(data.amount);
        }
    }
}