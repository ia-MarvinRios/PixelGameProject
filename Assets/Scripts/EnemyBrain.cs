using System.Linq;
using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    Transform[] _targetsTransform;
    public static EnemyBrain Instance { get; private set; }

    [Header("Enemy Settings")]
    [Space(10)]
    [Tooltip("Tag of the target the enemy will move towards.")]
    [SerializeField] string _targetTag = "Player";
    [SerializeField] float _moveSpeed = 5f;
    [SerializeField] int _damage = 10;

    public string TargetTag { get { return _targetTag; } set { _targetTag = value; } }
    public float MoveSpeed { get { return _moveSpeed; } }

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        StoreTargetsTransform();
    }

    void StoreTargetsTransform()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(_targetTag);
        _targetsTransform = objs.Select(o => o.transform).ToArray();
    }

    public Transform GetNearestTargetPosition(Transform enemy)
    {
        float nearest = 999999;
        Transform target = null;

        if (_targetsTransform.Length > 0)
        {
            for (int i = 0; i < _targetsTransform.Length; i++)
            {
                float current = Vector3.Distance(enemy.transform.position, _targetsTransform[i].transform.position);
                if (current < nearest)
                {
                    nearest = current;
                    target = _targetsTransform[i];
                }
            }
        }
        else
        {
            Debug.LogWarning($"No objects with tag {_targetTag} found in the scene.");
        }

        return target;
    }
}
