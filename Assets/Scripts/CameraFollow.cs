using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target & Offset")]
    public Transform target;                 // Player
    public Vector3 offset = new Vector3(0f, 3.5f, -6f);

    [Header("Smoothing")]
    [Range(0f, 20f)] public float followSpeed = 10f;   // mượt vị trí
    [Range(0f, 20f)] public float rotateSpeed = 10f;   // mượt xoay
    public bool lookAtTarget = true;

    [Header("Anti-Clip (chống camera chui vào tường)")]
    public LayerMask collisionMask;          // layer tường, địa hình
    public float minDistance = 1.0f;         // khoảng cách tối thiểu tới target
    public float wallPadding = 0.2f;         // đệm khỏi tường

    Vector3 _vel;

    void LateUpdate()
    {
        if (target == null) return;

        // Vị trí mong muốn (theo offset local của target)
        Vector3 desiredPos = target.TransformPoint(offset);

        // Raycast từ target ra vị trí mong muốn để tránh xuyên tường
        Vector3 targetPos = target.position + Vector3.up * 1.6f; // tầm mắt
        Vector3 dir = (desiredPos - targetPos);
        float dist = dir.magnitude;
        if (dist > 0.001f)
        {
            dir /= dist;
            if (Physics.SphereCast(targetPos, 0.2f, dir, out RaycastHit hit, dist, collisionMask, QueryTriggerInteraction.Ignore))
            {
                desiredPos = hit.point - dir * wallPadding;
                // bảo đảm không quá sát người
                Vector3 minPos = targetPos + dir * minDistance;
                if (Vector3.Distance(targetPos, desiredPos) < minDistance)
                    desiredPos = minPos;
            }
        }

        // Di chuyển mượt
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _vel, 1f / Mathf.Max(0.0001f, followSpeed));

        // Quay mượt nhìn về nhân vật (tuỳ chọn)
        if (lookAtTarget)
        {
            Quaternion targetRot = Quaternion.LookRotation((targetPos - transform.position).normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotateSpeed);
        }
    }
}
