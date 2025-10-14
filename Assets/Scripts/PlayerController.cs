using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Stats Settings")]
    [SerializeField] private float _speed = 2f;

    private Vector2 _direction;
    private Vector3 _mousePos;
    private PlayerInput _inputs;
    private Rigidbody2D _rb2D;
    private Animator _animator;

    private void Awake()
    {
        // Get Components
        _inputs = GetComponent<PlayerInput>();
        _rb2D = GetComponent<Rigidbody2D>();
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
        _mousePos = Camera.main.ScreenToWorldPoint(new Vector3(_inputs.actions["Aim"].ReadValue<Vector2>().x, _inputs.actions["Aim"].ReadValue<Vector2>().y, Camera.main.nearClipPlane));
    }

    void Move()
    {
        _rb2D.linearVelocity = _direction * _speed;

        // Update Animator Parameters
        _animator.SetFloat("Velocity", _rb2D.linearVelocity.magnitude);
        _animator.SetFloat("InputX", _direction.x);
        _animator.SetFloat("InputY", _direction.y);
    }

    public void Shoot(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            // Shoot logic
            Debug.Log("Pew Pew");
        }
    }
}