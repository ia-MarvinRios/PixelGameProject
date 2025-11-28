using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] Rigidbody2D _rb2d;
    [SerializeField] float _speed = 5f;
    [SerializeField] float _lifeTime = 1.5f;
    Vector2 _direction;

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("SolidObstacle") || collision.gameObject.CompareTag("Bomb"))
        {
            StartCoroutine(DisableOnNextFrame());
        }
    }

    public void Initialize(Vector2 dir)
    {
        _direction = dir.normalized;
        _rb2d.linearVelocity = _direction * _speed;
        StartCoroutine(LifeTimeHandler());
    }

    IEnumerator LifeTimeHandler()
    {
        yield return new WaitForSeconds(_lifeTime);
        ObjectPoolManager.ReturnObjectToPool(gameObject, ObjectPoolManager.PoolType.BasicBullet01);
    }
    IEnumerator DisableOnNextFrame()
    {
        yield return null;
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }
}