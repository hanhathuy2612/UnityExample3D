using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMoveToCamera : MonoBehaviour
{
    [Header("Move")] [SerializeField] private float maxSpeed = 6f;
    [SerializeField] float accel = 20f;

    [Header("Rotate")] [SerializeField] private float turnSpeed = 10f; // độ/giây

    private Rigidbody _rb;
    private Vector3 _inputDir;
    private Camera _camera;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _camera = Camera.main;
    }

    void Update()
    {
        if (Keyboard.current == null)
        {
            _inputDir = Vector3.zero;
            return;
        }

        float x = (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0);
        float z = (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0);

        var cam = _camera ? _camera.transform : null;
        if (cam)
        {
            Vector3 fwd = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
            _inputDir = (right * x + fwd * z).normalized;
        }
        else
        {
            _inputDir = new Vector3(x, 0f, z).normalized;
        }
    }

    void FixedUpdate()
    {
        Vector3 targetVel = _inputDir * maxSpeed;
        Vector3 horizVel = Vector3.ProjectOnPlane(_rb.linearVelocity, Vector3.up);
        Vector3 needed = targetVel - horizVel;
        _rb.AddForce(needed * accel, ForceMode.Acceleration);

        if (_inputDir.sqrMagnitude > 0.0001f)
        {
            float yaw = Mathf.Atan2(_inputDir.x, _inputDir.z) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0f, yaw, 0f);
            _rb.MoveRotation(Quaternion.RotateTowards(_rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime));
        }
    }
}