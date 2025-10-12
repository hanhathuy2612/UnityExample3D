using System;
using Unity.Constants;
using Unity.Core;
using UnityEngine;
using UnityEngine.AI;
using static Unity.Player.PlayerDamageTaken;

namespace Unity.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        public enum State
        {
            Patrol,
            Chase,
            Attack,
            Dead
        }

        public State state = State.Patrol;

        [Header("References")] public Transform[] patrolPoints;
        public Transform player; // set in inspector or find in Start
        public NavMeshAgent agent;
        public Animator animator; // optional
        public int enemyDamage = 5;

        [Header("Stats")] public float sightRadius = 12f;
        public float attackRange = 2f; // melee range
        public float timeBetweenAttacks = 0.5f;
        public bool isRanged = false;
        public GameObject projectilePrefab; // for ranged
        public Transform projectileSpawnPoint;
        public float projectileSpeed = 15f;

        [Header("Patrol")] public float patrolStoppingDistance = 0.5f;
        private int _patrolIndex = 0;

        private float _lastAttackTime = -999f;
        private EnemyHealth _healthComponent;

        private void Reset()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Awake()
        {
            if (agent == null) agent = GetComponent<NavMeshAgent>();
            if (animator == null) animator = GetComponent<Animator>();
        }

        private void Start()
        {
            if (player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p) player = p.transform;
            }

            _healthComponent = GetComponent<EnemyHealth>();
            if (_healthComponent) _healthComponent.OnDeath += HandleDeath;

            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                // fallback: stay in place
                patrolPoints = new[] { transform };
            }

            GoToNextPatrolPoint();
        }

        private void Update()
        {
            if (state == State.Dead) return;
            if (!player) return;

            float distToPlayer = Vector3.Distance(transform.position, player.position);

            // State decisions
            State previousState = state;
            if (distToPlayer <= attackRange)
            {
                state = State.Attack;
            }
            else if (distToPlayer <= sightRadius)
            {
                state = State.Chase;
            }
            else
            {
                state = State.Patrol;
            }

            // Debug state changes
            if (previousState != state)
            {
                // Debug.Log($"Enemy {gameObject.name} changed state from {previousState} to {state}. Distance to player: {distToPlayer:F2}");
            }

            switch (state)
            {
                case State.Patrol:
                    PatrolUpdate();
                    break;
                case State.Chase:
                    ChaseUpdate();
                    break;
                case State.Attack:
                    AttackUpdate(distToPlayer);
                    break;
            }

            // Animator example
            if (animator)
            {
                animator.SetFloat(AnimatorParameters.Speed, agent.velocity.magnitude);
                animator.SetBool("Dead", state == State.Dead);
            }
        }

        private void PatrolUpdate()
        {
            if (agent.remainingDistance <= patrolStoppingDistance || !agent.hasPath)
            {
                GoToNextPatrolPoint();
            }
        }

        private void GoToNextPatrolPoint()
        {
            if (patrolPoints.Length == 0) return;
            _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
            agent.isStopped = false;
            agent.SetDestination(patrolPoints[_patrolIndex].position);
        }

        private void ChaseUpdate()
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

        private void AttackUpdate(float distToPlayer)
        {
            // Check if player moved out of attack range
            if (distToPlayer > attackRange)
            {
                // Player moved away, go back to chase
                return;
            }

            agent.isStopped = true;
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z)); // face player

            if (Time.time - _lastAttackTime >= timeBetweenAttacks)
            {
                _lastAttackTime = Time.time;
                DoAttack();
            }
        }

        private void DoAttack()
        {
            // Animator trigger
            if (animator) animator.SetTrigger(AnimatorParameters.Attack);

            if (isRanged && projectilePrefab && projectileSpawnPoint)
            {
                GameObject p = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
                Projectile proj = p.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.Initialize(player.position, projectileSpeed, gameObject);
                }
                else
                {
                    Rigidbody rb = p.GetComponent<Rigidbody>();
                    if (rb)
                        rb.linearVelocity = (player.position - projectileSpawnPoint.position).normalized *
                                            projectileSpeed;
                }
            }
            else
            {
                // melee: apply damage if still in range
                float dist = Vector3.Distance(transform.position, player.position);
                if (dist <= attackRange + 0.5f)
                {
                    player.SendMessage("TakeDamage", enemyDamage, SendMessageOptions.DontRequireReceiver);
                    if (animator) animator.SetTrigger(AnimatorParameters.Punch);
                }
            }
        }

        private void HandleDeath()
        {
            state = State.Dead;
            agent.isStopped = true;
            agent.ResetPath();
            if (animator) animator.SetTrigger("Die");
            Destroy(gameObject, 6f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}