using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float accel = 20f;
    
    [SerializeField] private float turnSpeed = 540f;

    private Rigidbody _rb;
    private Animator _anim;
    private Vector3 _inputDir;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        _anim = GetComponentInChildren<Animator>();
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
        _inputDir = new Vector3(x, 0, z).normalized;

        float speed = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z).magnitude;
        _anim?.SetFloat("Speed", speed);
    }

    private void FixedUpdate()
    {
        Move();
        Rotate();
    }

    private void Move()
    {
        Vector3 targetVel = _inputDir * maxSpeed;
        Vector3 horizVel = Vector3.ProjectOnPlane(_rb.linearVelocity, Vector3.up);
        Vector3 needed = targetVel - horizVel;
        _rb.AddForce(needed * accel, ForceMode.Acceleration);
    }

    private void Rotate()
    {
        if (_inputDir.sqrMagnitude > 0.0001f)
        {
            float yaw = Mathf.Atan2(_inputDir.x, _inputDir.z) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0f, yaw, 0f);
            _rb.MoveRotation(Quaternion.RotateTowards(_rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime));
        }
    }
}