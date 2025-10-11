using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Targeting")]
        public LayerMask enemyMask;
        public float searchRadius = 10f;
        public bool requireLineOfSight = true;
        public LayerMask obstacleMask;

        [Header("Turning")]
        public Transform turnPivot;
        public float turnSpeedDegPerSec = 720f;
        public bool instantTurnOnAttack = false;

        private void Start()
        {
            turnPivot = transform;
        }

        private void Update()
        {
            if (Keyboard.current.eKey.wasReleasedThisFrame || Keyboard.current.qKey.wasReleasedThisFrame)
            {
                OnAttackInput();
            }
        }

        public void OnAttackInput()
        {
            Transform target = FindNearestEnemy();
            if (target != null)
            {
                FaceTarget(target, instantTurnOnAttack);
            }
            else
            {
                Debug.Log("No enemy found");
            }
        }

        Transform FindNearestEnemy()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, enemyMask);
            Debug.Log("Found " + hits.Length + " enemies");
            Transform best = null;
            float bestSqr = float.PositiveInfinity;

            foreach (var h in hits)
            {
                Transform t = h.attachedRigidbody ? h.attachedRigidbody.transform : h.transform;

                if (requireLineOfSight)
                {
                    Vector3 origin = transform.position + Vector3.up * 1.6f;
                    Vector3 dest = h.bounds.center;
                    Vector3 dir = dest - origin;
                    if (Physics.Raycast(origin, dir.normalized, out RaycastHit rh, dir.magnitude, obstacleMask))
                        continue;
                }

                float sqr = (t.position - transform.position).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = t;
                }
            }

            return best;
        }

        void FaceTarget(Transform target, bool instant)
        {
            Vector3 dir = target.position - turnPivot.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;

            Quaternion look = Quaternion.LookRotation(dir);

            if (instant)
            {
                turnPivot.rotation = look;
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(RotateTowards(look));
            }
        }

        IEnumerator RotateTowards(Quaternion targetRot)
        {
            while (Quaternion.Angle(turnPivot.rotation, targetRot) > 0.5f)
            {
                turnPivot.rotation = Quaternion.RotateTowards(
                    turnPivot.rotation, targetRot, turnSpeedDegPerSec * Time.deltaTime);
                yield return null;
            }
            turnPivot.rotation = targetRot;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, searchRadius);
        }
    }
}