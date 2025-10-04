using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMoveToCamera : MonoBehaviour
{
    [Header("Move")] [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float accel = 20f;

    [Header("Rotate")] [SerializeField] private float turnSpeed = 10f; // độ/giây

    private Rigidbody _rb;
    private Vector3 _inputDir;
    private Camera _camera;
    private float _actualSpeed;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _camera = Camera.main;
    }

    private void Update()
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
            var fwd = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            var right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
            _inputDir = (right * x + fwd * z).normalized;
        }
        else
        {
            _inputDir = new Vector3(x, 0f, z).normalized;
        }

        // Press and hold the shift key to speed up
        if (Keyboard.current.shiftKey.isPressed)
        {
            _actualSpeed = maxSpeed * 2f;
        }
        else
        {
            _actualSpeed = maxSpeed;
        }
    }

    private void FixedUpdate()
    {
        Move();
        TurnAround();
    }

    private void Move()
    {
        var targetVel = _inputDir * _actualSpeed;
        var horizVel = Vector3.ProjectOnPlane(_rb.linearVelocity, Vector3.up);
        var needed = targetVel - horizVel;
        _rb.AddForce(needed * accel, ForceMode.Acceleration);
    }

    private void TurnAround()
    {
        if (_inputDir.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        var yaw = Mathf.Atan2(_inputDir.x, _inputDir.z) * Mathf.Rad2Deg;
        var targetRot = Quaternion.Euler(0f, yaw, 0f);
        _rb.MoveRotation(Quaternion.RotateTowards(_rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime));
    }
}