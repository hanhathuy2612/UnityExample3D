using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Character controller that moves in world space,
/// accelerates toward a target horizontal velocity, rotates toward move direction,
/// and supports buffered/coyote jumping with a Rigidbody.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PlayerMoveToCamera : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float maxSpeed = 2f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float sprintMultiplier = 1.8f;

    [Header("Rotate")]
    [Tooltip("Degrees per second - how fast player turns")]
    [SerializeField] private float turnSpeed = 360f;

    [Header("Jump")]
    [Tooltip("Upward speed applied when jumping (m/s). If you prefer height, see notes below.")]
    [SerializeField] private float jumpSpeed = 5f;
    [SerializeField] private float coyoteTime = 0.1f;       // grace period after leaving ground
    [SerializeField] private float jumpBufferTime = 0.1f;   // early button press buffer
    [SerializeField] private LayerMask groundMask = ~0;     // what counts as ground

    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float groundCheckOffset = 0.05f;

    private Rigidbody rb;
    private Collider col;

    // input state (from Update)
    private Vector2 moveInput;          // XZ movement (-1..1)
    private bool wantsJump;             // edge-triggered
    private bool sprintHeld;

    // derived / runtime
    private float lastGroundedTime;
    private float lastJumpPressedTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Update()
    {
        // --- Read input (new Input System via Keyboard fallback) ---
        if (Keyboard.current != null)
        {
            var kbd = Keyboard.current;
            moveInput.x = (kbd.dKey.isPressed ? 1 : 0) - (kbd.aKey.isPressed ? 1 : 0);
            moveInput.y = (kbd.wKey.isPressed ? 1 : 0) - (kbd.sKey.isPressed ? 1 : 0);
            sprintHeld = kbd.shiftKey.isPressed;

            // buffer jump presses so we don't miss them between Update/FixedUpdate
            if (kbd.spaceKey.wasPressedThisFrame)
            {
                lastJumpPressedTime = Time.time;
                wantsJump = true;
            }
        }
        else
        {
            moveInput = Vector2.zero;
            sprintHeld = false;
        }
    }

    private void FixedUpdate()
    {
        // Ground probe first (affects jump & extra gravity feel if you add it later)
        bool grounded = CheckGrounded();
        if (grounded) lastGroundedTime = Time.time;

        MoveHorizontal();
        TryJump(grounded);
        rb.angularVelocity = Vector3.zero;
        TurnTowardMoveDirection();
    }

    // --- Movement ---
    private void MoveHorizontal()
    {
        // Desired horizontal direction (world space)
        Vector3 desiredDir = new Vector3(moveInput.x, 0f, moveInput.y);
        desiredDir = desiredDir.sqrMagnitude > 1f ? desiredDir.normalized : desiredDir;

        float targetSpeed = maxSpeed * (sprintHeld ? sprintMultiplier : 1f);
        Vector3 targetVel = desiredDir * targetSpeed;

        // Current horizontal velocity (strip Y)
        Vector3 vel = rb.linearVelocity;
        Vector3 horizVel = new Vector3(vel.x, 0f, vel.z);

        // Steering force toward target
        Vector3 needed = targetVel - horizVel;
        rb.AddForce(needed * acceleration, ForceMode.Acceleration);
    }

    // --- Jumping ---
    private void TryJump(bool grounded)
    {
        bool jumpBuffered = (Time.time - lastJumpPressedTime) <= jumpBufferTime;
        bool withinCoyote = (Time.time - lastGroundedTime) <= coyoteTime;

        if (wantsJump && (grounded || withinCoyote || jumpBuffered))
        {
            // set vertical velocity directly for clean, consistent jump
            Vector3 v = rb.linearVelocity;
            v.y = jumpSpeed;
            rb.linearVelocity = v;

            wantsJump = false;                 // consume
            lastJumpPressedTime = -999f;       // clear buffer
        }
        else if (wantsJump && !jumpBuffered)
        {
            // buffer expired; drop the intent
            wantsJump = false;
        }
    }

    // --- Rotation ---
    private void TurnTowardMoveDirection()
    {
        Vector3 planar = new Vector3(moveInput.x, 0f, moveInput.y);
        if (planar.sqrMagnitude < 0.0001f) return;

        float targetYaw = Mathf.Atan2(planar.x, planar.z) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.Euler(0f, targetYaw, 0f);

        // Smooth rotation like real human movement
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime));
    }

    // --- Ground Check ---
    private bool CheckGrounded()
    {
        // Sphere cast from collider bottom down a tiny bit
        Bounds b = col.bounds;
        float radius = groundCheckRadius;
        Vector3 origin = new Vector3(b.center.x, b.min.y + radius + 0.01f, b.center.z);

        // Short cast to detect ground just below collider
        float castDistance = radius + groundCheckOffset;
        if (Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, castDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            return true;
        }
        return false;
    }

    // --- Optional helper: turn jump height into speed ---
    // If you prefer "jumpHeight" instead of "jumpSpeed", compute it like:
    // jumpSpeed = Mathf.Sqrt(2f * Physics.gravity.magnitude * desiredHeight);
}
