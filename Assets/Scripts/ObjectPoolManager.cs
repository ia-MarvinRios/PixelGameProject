using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

public class ObjectPoolManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool _addToDontDestroyOnLoad = false;

    private GameObject _emptyHoder;

    // Categories of pools
    private static GameObject _gameObjectsEmpty;
    private static GameObject _basicBullet01;
    private static GameObject _enemySlime;

    // Dictionaries
    private static Dictionary<GameObject, ObjectPool<GameObject>> _objectPools;
    private static Dictionary<GameObject, GameObject> _cloneToPrefabMap;

    public enum PoolType
    {
        GameObjects,
        BasicBullet01,
        enemySlime,
    }
    public static PoolType PoolingType;

    private void Awake()
    {
        _objectPools = new Dictionary<GameObject, ObjectPool<GameObject>>();
        _cloneToPrefabMap = new Dictionary<GameObject, GameObject>();

        SetupEmpties();
    }

    void SetupEmpties()
    {
        _emptyHoder = new GameObject("Object Pools");

        // Set the categories children of the empty holder
        _gameObjectsEmpty = new GameObject("GameObjects");
        _gameObjectsEmpty.transform.SetParent(_emptyHoder.transform);

        _basicBullet01 = new GameObject("BasicBullet01");
        _basicBullet01.transform.SetParent(_emptyHoder.transform);

        _enemySlime = new GameObject("EnemySlime");
        _enemySlime.transform.SetParent(_emptyHoder.transform);

        if (_addToDontDestroyOnLoad)
            DontDestroyOnLoad(_gameObjectsEmpty.transform.root);
    }

    static void CreatePool(GameObject prefab, Vector3 pos, Quaternion rot, PoolType poolType = PoolType.GameObjects)
    {
        ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            createFunc: () => CreateObject(prefab, pos, rot, poolType),
            actionOnGet: OnGetObject,
            actionOnRelease: OnReleaseObject,
            actionOnDestroy: OnDestroyObject
            );

        _objectPools.Add(prefab, pool);
    }

    static GameObject CreateObject(GameObject prefab, Vector3 pos, Quaternion rot, PoolType poolType = PoolType.GameObjects)
    {
        prefab.SetActive(false);

        GameObject obj = Instantiate(prefab, pos, rot);

        prefab.SetActive(true);

        GameObject parentObject = SetParentObject(poolType);
        obj.transform.SetParent(parentObject.transform);

        return obj;
    }

    static void OnGetObject(GameObject obj)
    {

    }

    static void OnReleaseObject(GameObject obj)
    {
        obj.SetActive(false);
    }

    static void OnDestroyObject(GameObject obj)
    {
        if (_cloneToPrefabMap.ContainsKey(obj))
        {
            _cloneToPrefabMap.Remove(obj);
        }
    }

    static GameObject SetParentObject(PoolType poolType)
    {
        switch (poolType)
        {
            case PoolType.GameObjects:
                return _gameObjectsEmpty;

            case PoolType.BasicBullet01:
                return _basicBullet01;

            case PoolType.enemySlime:
                return _enemySlime;

            default:
                return null;
        }
    }

    static T SpawnObject<T>(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRot, PoolType poolType = PoolType.GameObjects) where T : Object
    {
        if (!_objectPools.ContainsKey(objectToSpawn))
        {
            CreatePool(objectToSpawn, spawnPos, spawnRot, poolType);
        }

        GameObject obj = _objectPools[objectToSpawn].Get();

        if (obj != null)
        {
            if (!_cloneToPrefabMap.ContainsKey(obj))
            {
                _cloneToPrefabMap.Add(obj, objectToSpawn);
            }

            obj.transform.position = spawnPos;
            obj.transform.rotation = spawnRot;
            obj.SetActive(true);

            if (typeof(T) == typeof(GameObject))
            {
                return obj as T;
            }

            T component = obj.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"Object {obj.name} does NOT have a componen of type {typeof(T)}");
                return null;
            }

            return component;
        }

        return null;
    }

    public static T SpawnObject<T>(T typePrefab, Vector3 spawnPos, Quaternion spawnRot, PoolType poolType = PoolType.GameObjects) where T : Component
    {
        return SpawnObject<T>(typePrefab.gameObject, spawnPos, spawnRot, poolType);
    }
    public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRot, PoolType poolType = PoolType.GameObjects)
    {
        return SpawnObject<GameObject>(objectToSpawn, spawnPos, spawnRot, poolType);
    }
    public static void ReturnObjectToPool(GameObject obj, PoolType poolType = PoolType.GameObjects)
    {
        if (_cloneToPrefabMap.TryGetValue(obj, out GameObject prefab))
        {
            GameObject parentObject = SetParentObject(poolType);

            if (obj.transform.parent != parentObject.transform)
            {
                obj.transform.SetParent(parentObject.transform);
            }
            if (_objectPools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
            {
                pool.Release(obj);
            }
        }
        else
        {
            Debug.LogWarning($"Trying to return an object that is not pooled: {obj.name}");
        }

    }

}
