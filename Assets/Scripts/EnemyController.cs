using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    private List<Vector3> _path;
    private int _currentPathIndex;
    Rigidbody2D _rb;

    [Header("Enemy Settings")]
    [SerializeField, Range(0.1f, 2f)] float _repathRate = 0.5f;

    private void OnDisable()
    {
        _rb = null;
        StopAllCoroutines();
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        StartCoroutine(UpdateTargetPos());
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
                if (_currentPathIndex >= _path.Count)
                {
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
        if (_path == null)
        {
            Debug.LogWarning($"<color=orange>Enemy at {transform.position} couldn't find a path to target at {targetPosition}!</color>");
            StopCoroutine(UpdateTargetPos());
            StopMoving();
        }
    }

    IEnumerator UpdateTargetPos()
    {
        while (true)
        {
            SetTargetPosition(EnemyBrain.Instance.GetNearestTargetPosition(transform).position);
            yield return new WaitForSeconds(_repathRate);
        }
    }
}
