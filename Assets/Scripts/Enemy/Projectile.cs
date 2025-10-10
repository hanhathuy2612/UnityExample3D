using UnityEngine;
using static Unity.Player.PlayerDamageTaken;

namespace Unity.Enemy
{
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Collider))]
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        public float speed = 100f;
        public float lifeTime = 5f;
        public int damage = 10;
        public GameObject owner;
        
        [Header("Performance")]
        public LayerMask playerLayer = 1; // Default layer
        public bool useLayerCheck = true;
        public bool useObjectPooling = false; // Enable for object pooling
        
        // Cached components
        private Collider _collider;
        private Rigidbody _rigidbody;
        
        // Cached values
        private int _playerLayerMask;
        private bool _isPlayerLayerCached;
        
        // Object pooling
        private bool _isActive = true;

        void Start()
        {
            // Cache components once
            CacheComponents();
            
            // Setup collider as trigger
            SetupTriggerCollider();

            // Destroy after lifetime
            Destroy(gameObject, lifeTime);
        }
        
        private void CacheComponents()
        {
            _collider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();
            
            // Cache player layer mask
            if (useLayerCheck)
            {
                _playerLayerMask = playerLayer.value;
                _isPlayerLayerCached = true;
            }
        }

        private void SetupTriggerCollider()
        {
            if (_collider != null && !_collider.isTrigger)
            {
                _collider.isTrigger = true;
                Debug.Log("Projectile collider set as trigger");
            }
            else if (_collider == null)
            {
                Debug.LogError("Projectile is missing a Collider component! Please add one in the Inspector.");
            }
        }

        void Update()
        {
            // Early exit if not active (for object pooling)
            if (!_isActive) return;
            
            // Use cached transform and multiply by Time.deltaTime once
            float deltaTime = Time.deltaTime;
            transform.Translate(Vector3.forward * speed * deltaTime);
        }

        public void Initialize(Vector3 target, float speed, GameObject owner)
        {
            this.speed = speed;
            this.owner = owner;
            transform.LookAt(target);
        }

        private void OnTriggerEnter(Collider other)
        {
            // Early exit if not active or hitting owner
            if (!_isActive || other.gameObject == owner)
                return;

            // Fast layer check first (bitwise operation)
            if (_isPlayerLayerCached)
            {
                if ((1 << other.gameObject.layer & _playerLayerMask) == 0)
                    return;
            }
            else
            {
                // Fallback to tag check if layer check is disabled
                if (!other.gameObject.CompareTag("Player"))
                    return;
            }

            // Create damage data once and reuse
            DealDamage(other);
        }
        
        private void DealDamage(Collider target)
        {
            var damageData = new DamageTakenData
            {
                amount = damage,
                attacker = owner
            };
            
            target.SendMessage("TakeDamage", damageData, SendMessageOptions.DontRequireReceiver);
            
            // Destroy or deactivate projectile after dealing damage
            if (useObjectPooling)
            {
                DeactivateProjectile();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void DeactivateProjectile()
        {
            _isActive = false;
            gameObject.SetActive(false);
        }
        
        public void ReactivateProjectile()
        {
            _isActive = true;
            gameObject.SetActive(true);
        }
    }

}