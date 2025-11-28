using System.Collections;
using System.Linq;
using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    int _enemyCount = 0;
    Transform[] _targetsTransform;

    [Header("Enemy Settings")]
    [Space(10)]
    [SerializeField] int _maxLevelEntities = 10;
    [SerializeField] float _spawnRate = 2f;
    [Tooltip("Tag of the target the enemy will move towards.")]
    [SerializeField] string _targetTag = "Player";
    [Tooltip("The basic movement speed of the enemy. Can be changed by modifiers.")]
    [SerializeField] float _moveSpeed = 5f;
    [Header("Spawn Settings")]
    [Space(10)]
    [SerializeField] Transform[] _spawnPoints;
    [SerializeField] GameObject[] _enemyPrefabs;

    public static EnemyBrain Instance { get; private set; }
    public string TargetTag { get { return _targetTag; } set { _targetTag = value; } }
    public float MoveSpeed { get { return _moveSpeed; } }
    public int EnemyCount { get { return _enemyCount; } set { _enemyCount = value; } }

    private void Awake()
    {
        Instance = this;
    }
    private void OnEnable()
    {
        // Events
        EnemyController.onEnemyAttack += HandleEnemyAttack;
        EnemyController.onEnemyDie += HandleEnemyDeath;
    }
    private void OnDisable()
    {
        // Events
        EnemyController.onEnemyAttack -= HandleEnemyAttack;
        EnemyController.onEnemyDie -= HandleEnemyDeath;
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
    GameObject HandleEnemyDeath(GameObject enemy)
    {
        ObjectPoolManager.ReturnObjectToPool(enemy, ObjectPoolManager.PoolType.enemies);

        // Return enemy's HealthBar to the pool
        EnemyController ctrl = enemy.GetComponent<EnemyController>();
        if (ctrl != null) ObjectPoolManager.ReturnObjectToPool(ctrl.HealthBar.gameObject, ObjectPoolManager.PoolType.EnemyHealthBars);
        else Debug.LogWarning($"Couldn't return {enemy}'s HealthBar to the pool.");

        return enemy;
    }

    IEnumerator SpawnEnemies()
    {
        yield return null;
        while (GameManager.Instance.TimeOnLevel < GameManager.Instance.LevelTargetTime * 60)
        {
            if (_enemyCount < _maxLevelEntities)
            {
                foreach (Transform spawnPoint in _spawnPoints)
                {
                    int a = Random.Range(0, _enemyPrefabs.Length);
                    ObjectPoolManager.SpawnObject(_enemyPrefabs[a], spawnPoint.position, spawnPoint.rotation, ObjectPoolManager.PoolType.enemies);
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
