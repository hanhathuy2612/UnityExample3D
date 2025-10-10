using Unity.Constants;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Character controller that moves in world space,
/// accelerates toward a target horizontal velocity, rotates toward move direction,
/// and supports buffered/coyote jumping with a Rigidbody.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Move")][SerializeField] private float maxSpeed = 2f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float sprintMultiplier = 1.8f;

    [Header("Rotate")]
    [Tooltip("Degrees per second - how fast player turns")]
    [SerializeField]
    private float turnSpeed = 360f;

    [Header("Jump")]
    [Tooltip("Upward speed applied when jumping (m/s). If you prefer height, see notes below.")]
    [SerializeField]
    private float jumpSpeed = 8f;

    [SerializeField] private float coyoteTime = 0.1f; // grace period after leaving ground
    [SerializeField] private float jumpBufferTime = 0.1f; // early button press buffer
    [SerializeField] private LayerMask groundMask = ~0; // what counts as ground

    [Header("Ground Check")]
    [SerializeField]
    private float groundCheckRadius = 0.2f;

    [SerializeField] private float groundCheckOffset = 0.1f;
    [SerializeField] private bool showGroundCheckDebug = true;
    [SerializeField] private bool useSimpleGroundCheck = true;

    private Rigidbody _rb;
    private Collider _col;

    // input state (from Update)
    private Vector2 _moveInput; // XZ movement (-1..1)
    private bool _wantsJump; // edge-triggered
    private bool _sprintHeld;

    // derived / runtime
    private float _lastGroundedTime;
    private float _lastJumpPressedTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();

        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Update()
    {
        // --- Read input (new Input System via Keyboard fallback) ---
        if (Keyboard.current != null)
        {
            var kbd = Keyboard.current;
            _moveInput.x = (kbd.dKey.isPressed ? 1 : 0) - (kbd.aKey.isPressed ? 1 : 0);
            _moveInput.y = (kbd.wKey.isPressed ? 1 : 0) - (kbd.sKey.isPressed ? 1 : 0);
            _sprintHeld = kbd.shiftKey.isPressed;

            // buffer jump presses so we don't miss them between Update/FixedUpdate
            if (kbd.spaceKey.wasPressedThisFrame)
            {
                _lastJumpPressedTime = Time.time;
                _wantsJump = true;
            }
        }
        else
        {
            _moveInput = Vector2.zero;
            _sprintHeld = false;
        }
    }

    private void FixedUpdate()
    {
        // Ground probe first (affects jump & extra gravity feel if you add it later)
        bool grounded = CheckGrounded();
        if (grounded) _lastGroundedTime = Time.time;

        MoveHorizontal();
        TryJump(grounded);
        _rb.angularVelocity = Vector3.zero;
        TurnTowardMoveDirection();
    }

    // --- Movement ---
    private void MoveHorizontal()
    {
        // Desired horizontal direction (world space)
        Vector3 desiredDir = new Vector3(_moveInput.x, 0f, _moveInput.y);
        desiredDir = desiredDir.sqrMagnitude > 1f ? desiredDir.normalized : desiredDir;

        float targetSpeed = maxSpeed * (_sprintHeld ? sprintMultiplier : 1f);
        Vector3 targetVel = desiredDir * targetSpeed;

        // Current horizontal velocity (strip Y)
        Vector3 vel = _rb.linearVelocity;
        Vector3 horizVel = new Vector3(vel.x, 0f, vel.z);

        // Steering force toward target
        Vector3 needed = targetVel - horizVel;
        _rb.AddForce(needed * acceleration, ForceMode.Acceleration);
    }

    // --- Jumping ---
    private void TryJump(bool grounded)
    {
        bool jumpBuffered = (Time.time - _lastJumpPressedTime) <= jumpBufferTime;
        bool withinCoyote = (Time.time - _lastGroundedTime) <= coyoteTime;

        if (_wantsJump && (grounded || withinCoyote || jumpBuffered))
        {
            // set vertical velocity directly for clean, consistent jump
            Vector3 v = _rb.linearVelocity;
            v.y = jumpSpeed;
            _rb.linearVelocity = v;

            _wantsJump = false; // consume
            _lastJumpPressedTime = -999f; // clear buffer
        }
        else if (_wantsJump && !jumpBuffered)
        {
            // buffer expired; drop the intent
            _wantsJump = false;
        }
    }

    // --- Rotation ---
    private void TurnTowardMoveDirection()
    {
        Vector3 planar = new Vector3(_moveInput.x, 0f, _moveInput.y);
        if (planar.sqrMagnitude < 0.0001f) return;

        float targetYaw = Mathf.Atan2(planar.x, planar.z) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.Euler(0f, targetYaw, 0f);

        // Smooth rotation like real human movement
        _rb.MoveRotation(Quaternion.RotateTowards(_rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime));
    }

    // --- Ground Check ---
    private bool CheckGrounded()
    {
        if (useSimpleGroundCheck)
        {
            return CheckGroundedSimple();
        }
        else
        {
            return CheckGroundedSphereCast();
        }
    }

    private bool CheckGroundedSimple()
    {
        // Simple ground check using multiple raycasts
        Vector3 center = transform.position;
        Vector3[] checkPoints =
        {
            center,
            center + Vector3.left * 0.3f,
            center + Vector3.right * 0.3f,
            center + Vector3.forward * 0.3f,
            center + Vector3.back * 0.3f
        };

        float checkDistance = 0.6f; // Distance to check below player

        foreach (Vector3 point in checkPoints)
        {
            if (Physics.Raycast(point, Vector3.down, checkDistance, groundMask, QueryTriggerInteraction.Ignore))
            {
                if (showGroundCheckDebug)
                {
                    Debug.DrawRay(point, Vector3.down * checkDistance, Color.green);
                }

                return true;
            }
            else if (showGroundCheckDebug)
            {
                Debug.DrawRay(point, Vector3.down * checkDistance, Color.red);
            }
        }

        return false;
    }

    private bool CheckGroundedSphereCast()
    {
        // Sphere cast from collider bottom down a tiny bit
        Bounds b = _col.bounds;
        float radius = groundCheckRadius;
        Vector3 origin = new Vector3(b.center.x, b.min.y + radius + 0.01f, b.center.z);

        // Short cast to detect ground just below collider
        float castDistance = radius + groundCheckOffset;
        bool isGrounded = Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, castDistance, groundMask,
            QueryTriggerInteraction.Ignore);

        // Debug visualization
        if (showGroundCheckDebug)
        {
            Color debugColor = isGrounded ? Color.green : Color.red;
            Debug.DrawRay(origin, Vector3.down * castDistance, debugColor);

            // Draw sphere at origin
            Debug.DrawLine(origin + Vector3.left * radius, origin + Vector3.right * radius, debugColor);
            Debug.DrawLine(origin + Vector3.forward * radius, origin + Vector3.back * radius, debugColor);

            if (isGrounded)
            {
                Debug.Log($"Grounded! Hit: {hit.collider.name} at distance: {hit.distance}");
            }
        }

        return isGrounded;
    }
}