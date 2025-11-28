using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class ObjectPoolManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool _addToDontDestroyOnLoad = false;

    private GameObject _emptyHolder;
    private GameObject _canvasObj;

    // Categories of pools
    private static GameObject _gameObjectsEmpty;
    private static GameObject _basicBullet01;
    private static GameObject _enemies;
    private static GameObject _bosses;
    private static GameObject _enemyHealthBars;
    private static Canvas     _uiCanvas;

    // Dictionaries
    private static Dictionary<GameObject, ObjectPool<GameObject>> _objectPools;
    private static Dictionary<GameObject, GameObject> _cloneToPrefabMap;

    public enum PoolType
    {
        GameObjects,
        BasicBullet01,
        enemies,
        EnemyHealthBars,
        bosses,
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
        _emptyHolder = new GameObject("Object Pools");
        _canvasObj = new GameObject("UI_Canvas");
        SetUpUI();

        // Set the categories children of the empty holder
        _gameObjectsEmpty = new GameObject("GameObjects");
        _gameObjectsEmpty.transform.SetParent(_emptyHolder.transform);

        _basicBullet01 = new GameObject("BasicBullet01");
        _basicBullet01.transform.SetParent(_emptyHolder.transform);

        _enemies = new GameObject("Enemies");
        _enemies.transform.SetParent(_emptyHolder.transform);

        _bosses = new GameObject("Bosses");
        _bosses.transform.SetParent(_emptyHolder.transform);

        _enemyHealthBars = new GameObject("EnemyHealthBars");
        _enemyHealthBars.transform.SetParent(_canvasObj.transform);
        _enemyHealthBars.transform.localScale = Vector3.one;

        if (_addToDontDestroyOnLoad)
            DontDestroyOnLoad(_gameObjectsEmpty.transform.root);
    }
    void SetUpUI()
    {
        _uiCanvas = _canvasObj.AddComponent<Canvas>();
        _uiCanvas.renderMode = RenderMode.WorldSpace;

        CanvasScaler scaler = _canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 1;
        scaler.referencePixelsPerUnit = 32;

        _canvasObj.AddComponent<GraphicRaycaster>();

        RectTransform rt = _canvasObj.GetComponent<RectTransform>();
        rt.anchoredPosition3D = Vector3.zero;
        rt.sizeDelta = new Vector2(200, 30);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.zero;
        rt.localRotation = Quaternion.identity;
        rt.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        _canvasObj.transform.SetParent(_emptyHolder.transform);
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

            case PoolType.enemies:
                return _enemies;

            case PoolType.EnemyHealthBars:
                return _enemyHealthBars;

            case PoolType.bosses:
                return _bosses;

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
