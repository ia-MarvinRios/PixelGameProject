using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private Transform _target;
    [SerializeField] private float _speed = 1.5f;
    private Rigidbody2D rb2D;

    void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _target = other.transform;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _target = null;
        }
    }

    private void FixedUpdate()
    {
        MoveToTarget();
    }

    void MoveToTarget()
    {
        if (_target != null)
        {
            Vector2 direction = (_target.position - transform.position).normalized;
            rb2D.linearVelocity = direction * _speed;
        }
        else
        {
            rb2D.linearVelocity = Vector2.zero;
        }
    }
}
