using UnityEngine;

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
        float speed = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z).magnitude;
        Debug.Log(speed);
        _animator.SetFloat(AnimatorParameters.Speed, speed);
    }
}