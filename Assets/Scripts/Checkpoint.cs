using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CircleCollider2D))]
public class Checkpoint : MonoBehaviour
{
    public UnityEvent _onCheckpointTrigger;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _onCheckpointTrigger?.Invoke();
        }
    }
}
