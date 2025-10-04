using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerTransition : MonoBehaviour
{
    private Animator _animator;
    private Rigidbody _rigidbody;

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
    }

    private void Punch()
    {
        if (_animator.IsInTransition(0)) return;
        var state = _animator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName(AnimatorParameters.Punch)) return;
        _animator.SetTrigger(AnimatorParameters.Punch);
    }

    private void Jump()
    {
        if (_animator.IsInTransition(0)) return;
        var state = _animator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName(AnimatorParameters.Jump)) return;
        _animator.SetTrigger(AnimatorParameters.Jump);
    }
}