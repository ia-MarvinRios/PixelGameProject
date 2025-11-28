using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    private List<Vector3> _path;
    private int _currentPathIndex;
    Rigidbody2D _rb;
    CapsuleCollider2D _collider;
    Slider _healthBar;
    Coroutine _repathCoroutine;
    bool _isAttacking = false;
    float _health;

    [Header("Enemy Settings")]
    [SerializeField] Animator _animator;
    [SerializeField] Transform _healthBarAnchor;
    [SerializeField] GameObject _healthBarPrefab;
    [SerializeField, Range(0.1f, 2f)] float _repathRate = 0.5f;
    [SerializeField] float _attackRange = 1.5f;
    [SerializeField] float _attackCooldown = 2f;
    [SerializeField] float _attackDamage = 10f;
    [SerializeField] float _maxHealth = 10f;

    /// <summary>
    /// The current health value of the entity.
    /// </summary>
    public float Health { get { return _health; } }
    public Transform HealthBarAnchor {  get { return _healthBarAnchor; } }
    public Slider HealthBar { get { return _healthBar; } set { _healthBar = value; } }


    // Events
    public delegate void OnEnemyAttackEvent(float attackDamage);
    public static event OnEnemyAttackEvent onEnemyAttack;

    public delegate GameObject onEnemyDieEvent(GameObject enemy);
    public static event onEnemyDieEvent onEnemyDie;


    private void OnEnable()
    {
        SetUpReferences();

        _collider.enabled = true;
        _health = _maxHealth;
        
        SetUpHealthBar();

        _repathCoroutine = StartCoroutine(UpdateTargetPos());
    }
    private void OnDisable()
    {
        _rb = null;
        StopAllCoroutines();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Debug.Log($"Enemy {gameObject.name} was hit by {collision.gameObject.name}");

            TakeDamage(5);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        switch (other.gameObject.tag)
        {
            case "Bomb":
                if (other.isTrigger == false) return;
                if (_collider.IsTouching(other))
                    TakeDamage(other.GetComponent<Bomb>().Damage);
                break;

            default:
                break;
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        switch (other.gameObject.tag)
        {
            case "Player":
                if (Vector3.Distance(transform.position, other.transform.position) < _attackRange && !_isAttacking)
                {
                    StartCoroutine(Attack());
                }
                break;

            default:
                break;
        }
        
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    void SetUpReferences()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        }
        if (_animator == null) _animator = GetComponentInChildren<Animator>();
        if (_collider == null) _collider = GetComponent<CapsuleCollider2D>();
    }

    void HandleMovement()
    {

        if (_path != null)
        {
            Vector3 targetPosition = _path[_currentPathIndex];
            if (Vector3.Distance(transform.position, targetPosition) > 1f)
            {
                Vector3 moveDir = (targetPosition - transform.position).normalized;

                float distanceBefore = Vector3.Distance(transform.position, targetPosition);

                // Move towards the target position
                _rb.linearVelocity = moveDir * EnemyBrain.Instance.MoveSpeed * 0.1f;
            }
            else
            {
                _currentPathIndex++;
                if (_currentPathIndex >= _path.Count) {
                    //StopMoving();
                    _currentPathIndex = _path.Count - 1;
                    Debug.Log("Enemy on Target");
                }
            }
        }

        // Move Health Bar
        _healthBar.transform.position = _healthBarAnchor.transform.position;

        // Update walking animations
        _animator.SetFloat("VelocityX", _rb.linearVelocity.x);
        _animator.SetFloat("VelocityY", _rb.linearVelocity.y);
    }

    /// <summary>
    /// Stops the enemy's movement by clearing the path and setting velocity to zero.
    /// </summary>
    void StopMoving() {
        StopCoroutine(_repathCoroutine);
        _path = null;
        _rb.linearVelocity = Vector2.zero;
    }
    public void SetTargetPosition(Vector3 targetPosition)
    {
        _currentPathIndex = 0;
        _path = WalkableGrid.Instance.Walkable.FindPath(transform.position, targetPosition);
        if (_path == null) {
            Debug.LogWarning($"<color=orange>Enemy at {transform.position} couldn't find a path to target at {targetPosition}!</color>");
            //StopMoving();
        }
    }
    void TakeDamage(float dmg)
    {
        _health -= dmg;
        _healthBar.value = _health/_maxHealth;

        // Handle death
        if (_health <= 0f)
        {
            EnemyBrain.Instance.EnemyCount--;
            _collider.enabled = false;
            StopMoving();

            if (_animator != null)
                StartCoroutine(DieAnim());
            else
                onEnemyDie?.Invoke(gameObject);
        }
    }
    void SetUpHealthBar()
    {
        // Health Bar Reference
        _healthBar = ObjectPoolManager.SpawnObject(
                _healthBarPrefab,
                _healthBarAnchor.position,
                Quaternion.identity,
                ObjectPoolManager.PoolType.EnemyHealthBars).GetComponent<Slider>();

        _healthBar.transform.localScale = Vector3.one;
        _healthBar.value = _health;
    }



    // --- Coroutines ---

    IEnumerator DieAnim()
    {
        _animator.SetTrigger("Die");
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);
        onEnemyDie?.Invoke(gameObject);
    }

    IEnumerator UpdateTargetPos()
    {
        while (true)
        {
            yield return new WaitForSeconds(_repathRate);
            SetTargetPosition(EnemyBrain.Instance.GetNearestTargetPosition(transform).position);
        }
    }
    IEnumerator Attack()
    {
        _isAttacking = true;

        onEnemyAttack?.Invoke(_attackDamage);
        yield return new WaitForSeconds(_attackCooldown);

        _isAttacking = false;
    }

}
