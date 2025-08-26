using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Stats Settings")]
    [SerializeField] private float _speed = 2f;

    private Vector2 _direction;
    private PlayerInput _inputs;
    private Rigidbody2D rb2D;
    private Animator _animator;

    private void Awake()
    {
        // Get Components
        _inputs = GetComponent<PlayerInput>();
        rb2D = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        GetInputs();
    }

    private void FixedUpdate()
    {
        Move();
    }

    void GetInputs()
    {
        _direction = _inputs.actions["Move"].ReadValue<Vector2>();
    }

    void Move()
    {
        rb2D.linearVelocity = _direction * _speed;

        // Update Animator Parameters
        _animator.SetFloat("Velocity", rb2D.linearVelocity.magnitude);
        _animator.SetFloat("InputX", _direction.x);
        _animator.SetFloat("InputY", _direction.y);
    }
}