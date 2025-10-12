using Unity.Core;
using UnityEngine;

namespace Unity.Enemy
{
    public class EnemyDamageTaken : IDamageTaken
    {
        public override void TakeDamage(int amount)
        {
            if (healthBar.GetHealth() <= 0f)
            {
                return;
            }

            var currentHealth = healthBar.SetHealth(amount);

            if (currentHealth <= 0f)
            {
                gameObject.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}