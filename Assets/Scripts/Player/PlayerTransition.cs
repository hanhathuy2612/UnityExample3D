using Unity.Constants;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerTransition : MonoBehaviour
{
    public float timeBetweenPunches = 0.5f;
    public float timeBetweenKicks = 0.5f;
    public float timeBetweenJumps = 0.5f;

    private Animator _animator;
    private Rigidbody _rigidbody;

    private float _lastPunchTime;

    private float _lastKickTime;

    private float _lastJumpTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
        var speed = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z).magnitude;
        _animator.SetFloat(AnimatorParameters.Speed, speed);

        if (Keyboard.current.spaceKey.wasReleasedThisFrame)
        {
            Jump();
        }

        if (Keyboard.current.eKey.wasReleasedThisFrame)
        {
            Punch();
        }

        if (Keyboard.current.qKey.wasReleasedThisFrame)
        {
            Kick();
        }
    }

    private void Punch()
    {
        if (Time.time - _lastPunchTime < timeBetweenPunches) return;
        _lastPunchTime = Time.time;
        _animator.SetTrigger(AnimatorParameters.Punch);
    }

    private void Jump()
    {
        if (Time.time - _lastJumpTime < timeBetweenJumps) return;
        _lastJumpTime = Time.time;
        _animator.SetTrigger(AnimatorParameters.Jump);
    }

    private void Kick()
    {
        if (Time.time - _lastKickTime < timeBetweenKicks) return;
        _lastKickTime = Time.time;
        _animator.SetTrigger(AnimatorParameters.Kick);
    }
}