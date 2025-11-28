using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Bomb Settings")]
    [SerializeField] float _explosionTime = 1;
    [SerializeField] float _damage = 10;
    [SerializeField] CircleCollider2D _AreaEffect;
    [SerializeField] Animator _animator;
    [SerializeField] Rigidbody2D _rb;

    public float Damage{ get { return _damage; } }

    private void OnEnable()
    {
        StartCoroutine(CountdownExplode());
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator CountdownExplode()
    {
        yield return new WaitForSeconds(_explosionTime);
        _AreaEffect.enabled = true;

        _rb.linearVelocity = Vector3.zero;
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _animator.SetTrigger("Explode");
        AudioManager.Instance.PlaySoundByName("Explosion", gameObject.transform);
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);

        Destroy(gameObject);
    }
}
