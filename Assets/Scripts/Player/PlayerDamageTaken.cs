using Unity.Core;
using UnityEngine;

namespace Unity.Player
{
    public class PlayerDamageTaken : IDamageTaken
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