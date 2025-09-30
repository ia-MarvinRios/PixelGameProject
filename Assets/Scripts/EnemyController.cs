using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    private List<Vector3> _path;
    private int _currentPathIndex;
    Rigidbody2D _rb;
    bool _isAttacking = false;

    [Header("Enemy Settings")]
    [SerializeField, Range(0.1f, 2f)] float _repathRate = 0.5f;
    [SerializeField] float _attackRange = 1.5f;
    [SerializeField] float _attackCooldown = 2f;
    [SerializeField] float _attackDamage = 10f;

    public delegate void OnEnemyAttackEvent(float attackDamage);
    public static event OnEnemyAttackEvent onEnemyAttack;

    private void OnDisable()
    {
        _rb = null;
        StopAllCoroutines();
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
    }
    private void Start()
    {
        StartCoroutine(UpdateTargetPos());
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && Vector3.Distance(transform.position, other.transform.position) < _attackRange && !_isAttacking) {
            StartCoroutine(Attack());
        }
    }

    private void Update()
    {
        HandleMovement();
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
                    StopMoving();
                }
            }
        }
    }

    void StopMoving() {
        _path = null;
        _rb.linearVelocity = Vector2.zero;
    }
    public void SetTargetPosition(Vector3 targetPosition)
    {
        _currentPathIndex = 0;
        _path = WalkableGrid.Instance.Walkable.FindPath(transform.position, targetPosition);
        if (_path == null) {
            Debug.LogWarning($"<color=orange>Enemy at {transform.position} couldn't find a path to target at {targetPosition}!</color>");
            StopCoroutine(UpdateTargetPos());
            StopMoving();
        }
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
