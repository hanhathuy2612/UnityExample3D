using UnityEngine;

namespace Unity.Enemy
{
    public class Projectile : MonoBehaviour
    {
        public float lifeTime = 6f;
        public int damage = 10;
        Rigidbody rb;
        GameObject owner;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            Destroy(gameObject, lifeTime);
        }
        
        public void Initialize(Vector3 targetPos, float speed, GameObject owner)
        {
            this.owner = owner;
            Vector3 dir = (targetPos - transform.position).normalized;
            if (rb != null) rb.linearVelocity = dir * speed;
            transform.rotation = Quaternion.LookRotation(dir);
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == owner) return; // ignore owner
            // apply damage to player
            var ph = other.GetComponent<EnemyHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }
            else
            {
                other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }
            Destroy(gameObject);
        }
    }
}