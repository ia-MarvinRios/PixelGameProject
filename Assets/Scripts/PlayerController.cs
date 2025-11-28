using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] float _cooldown = 1f;
    [SerializeField] Image _aim;
    [SerializeField] GameObject _defaultWeapon;
    [SerializeField] Transform _weaponRoot;
    [SerializeField] Vector2 _orbitOffset = new Vector2(0.5f, 0.5f);
    [SerializeField] float _orbitRadius = 0.5f;
    [SerializeField] GameObject[] _bulletPrefabs;

    [Header("Stats Settings")]
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _bombCooldown = 3f;
    [SerializeField] private float _bombThrowingForce = 1f;

    private Vector2 _direction;
    private Vector2 _mousePos;
    private Vector3 _mouseWorldPos;
    private PlayerInput _inputs;
    private Rigidbody2D _rb2D;
    private Animator _animator;
    GameObject _currentWeapon;
    Transform _firePoint;
    private bool _isOnCooldown = false;
    private bool _isOnBombCooldown = false;

    // public properties
    public GameObject CurrentWeaponPrefab { get { return _currentWeapon; } set { _currentWeapon = value; } }

    private void Awake()
    {
        // Get Components
        _inputs = GetComponent<PlayerInput>();
        _rb2D = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();

    }

    private void Start()
    {
        // Initialize Weapon
        _currentWeapon = Instantiate(_defaultWeapon, _weaponRoot.transform.position, _weaponRoot.transform.rotation, transform);
        _firePoint = _currentWeapon.transform.Find("Firepoint");
    }

    private void OnDisable()
    {
        StopAllCoroutines();

    }

    private void Update()
    {
        GetInputs();
        MoveWeaponAndAim();
    }

    private void FixedUpdate()
    {
        Move();
    }

    void GetInputs()
    {
        _direction = _inputs.actions["Move"].ReadValue<Vector2>();
        _mousePos = _inputs.actions["Aim"].ReadValue<Vector2>();
        _mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(_inputs.actions["Aim"].ReadValue<Vector2>().x, _inputs.actions["Aim"].ReadValue<Vector2>().y, -Camera.main.transform.position.z));
    }

    void Move()
    {
        if (!GameManager.Instance.GameIsOver)
        {
            _rb2D.linearVelocity = _direction * _speed;

            // Update Animator Parameters
            _animator.SetFloat("Velocity", _rb2D.linearVelocity.magnitude);
            _animator.SetFloat("InputX", _direction.x);
            _animator.SetFloat("InputY", _direction.y);
        }
    }

    public void Shoot(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !_isOnCooldown && !GameManager.Instance.GameIsOver)
        {
            // Shoot logic
            AudioManager.Instance.PlaySoundByName("Shoot01", gameObject.transform);

            StartCoroutine(ShootCoroutine(0));
        }
    }
    public void ThrowBomb(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !_isOnBombCooldown && !GameManager.Instance.GameIsOver)
        {
            // Shoot logic
            AudioManager.Instance.PlaySoundByName("Throw", gameObject.transform);

            StartCoroutine(ShootCoroutine(1));
        }
    }

    void MoveWeaponAndAim()
    {
        if (!GameManager.Instance.GameIsOver)
        {
            // Move UI Aim
            _aim.rectTransform.position = _mousePos;

            // Move Weapon Orbit
            Vector3 dir = (_mouseWorldPos - _weaponRoot.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            Vector3 orbitCenter = _weaponRoot.position + (Vector3)_orbitOffset;

            _currentWeapon.transform.position = orbitCenter + dir * _orbitRadius;

            _currentWeapon.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    // --- Coroutines ---
    IEnumerator ShootCoroutine(int a)
    {

        switch(a)
        {
            case 0:
                _isOnCooldown = true;

                var bullet = ObjectPoolManager.SpawnObject(_bulletPrefabs[a], _firePoint.position, _firePoint.rotation, ObjectPoolManager.PoolType.BasicBullet01);
                bullet.GetComponent<Bullet>().Initialize((_mouseWorldPos - _firePoint.position).normalized);
                Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), GetComponent<Collider2D>());
                yield return new WaitForSeconds(_cooldown);

                _isOnCooldown = false;
                break;

            case 1:
                _isOnBombCooldown = true;

                var bomb = ObjectPoolManager.SpawnObject(_bulletPrefabs[a], _firePoint.position, _firePoint.rotation, ObjectPoolManager.PoolType.BasicBullet01);
                bomb.GetComponent<Rigidbody2D>().AddForce((_mouseWorldPos - _firePoint.position).normalized * _bombThrowingForce, ForceMode2D.Impulse);
                yield return new WaitForSeconds(_bombCooldown);

                _isOnBombCooldown = false;
                break;

            default:
                break;
        }

    }
}