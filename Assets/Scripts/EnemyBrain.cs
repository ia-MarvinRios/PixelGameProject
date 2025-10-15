using System.Collections;
using System.Linq;
using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    int _enemyCount = 0;
    Transform[] _targetsTransform;

    [Header("Enemy Settings")]
    [Space(10)]
    [SerializeField] float _spawnRate = 2f;
    [Tooltip("Tag of the target the enemy will move towards.")]
    [SerializeField] string _targetTag = "Player";
    [Tooltip("The basic movement speed of the enemy. Can be changed by modifiers.")]
    [SerializeField] float _moveSpeed = 5f;
    [Header("Spawn Settings")]
    [Space(10)]
    [SerializeField] Transform _enemyContainer;
    [SerializeField] Transform[] _spawnPoints;
    [SerializeField] GameObject[] _enemyPrefabs;

    public static EnemyBrain Instance { get; private set; }
    public string TargetTag { get { return _targetTag; } set { _targetTag = value; } }
    public float MoveSpeed { get { return _moveSpeed; } }

    private void Awake()
    {
        Instance = this;
    }
    private void OnEnable()
    {
        // Events
        EnemyController.onEnemyAttack += HandleEnemyAttack;
    }
    private void OnDisable()
    {
        // Events
        EnemyController.onEnemyAttack -= HandleEnemyAttack;
    }
    private void Start()
    {
        StoreTargetsTransform();
        StartCoroutine("SpawnEnemies");
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

    void HandleEnemyAttack(float damage)
    {
        GUIBrain.Instance.UpdateHealthBarByValue(-damage);
    }

    IEnumerator SpawnEnemies()
    {
        yield return null;
        while (GameManager.Instance.TimeOnLevel < 3 * 60)
        {
            if (_enemyCount < 10)
            {
                foreach (Transform spawnPoint in _spawnPoints)
                {
                    Instantiate(_enemyPrefabs[0], spawnPoint.position, spawnPoint.rotation, _enemyContainer);
                    _enemyCount++;
                }
                yield return new WaitForSeconds(_spawnRate);
            }
            else
            {
                Debug.Log("Max enemy count reached, waiting...");
                yield return new WaitForSeconds(_spawnRate);
            }
        }
    }
}
