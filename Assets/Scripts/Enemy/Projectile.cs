using UnityEngine;
using static Unity.Player.PlayerDamageTaken;

namespace Unity.Enemy
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Projectile : MonoBehaviour
    {
        public float speed = 100f;
        public float lifeTime = 5f;
        public int damage = 10;
        public GameObject owner;

        void Start()
        {
            // Ensure collider is set as trigger
            SetupTriggerCollider();

            Destroy(gameObject, lifeTime);
        }

        private void SetupTriggerCollider()
        {
            Collider col = GetComponent<Collider>();

            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
                Debug.Log("Projectile collider set as trigger");
            }
            else if (col == null)
            {
                Debug.LogError("Projectile is missing a Collider component! Please add one in the Inspector.");
            }
        }

        void Update()
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        public void Initialize(Vector3 target, float speed, GameObject owner)
        {
            this.speed = speed;
            this.owner = owner;
            transform.LookAt(target);
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"Projectile hit: {other.gameObject.name}");

            if (other.gameObject == owner)
            {
                Debug.Log("Projectile ignored owner collision");
                return;
            }

            if (IsPlayer(other))
            {
                var data = new DamageTakenData()
                {
                    amount = damage,
                    attacker = this.owner
                };
                other.SendMessage("TakeDamage", data, SendMessageOptions.DontRequireReceiver);
            }
        }

        private bool IsPlayer(Collider other)
        {
            return other.gameObject.layer == LayerMask.NameToLayer("Player") || other.gameObject.tag == "Player";
        }
    }

}