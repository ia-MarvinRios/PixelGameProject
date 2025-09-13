using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR

[CustomEditor(typeof(GridGenerator))]
public class GridGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GridGenerator gen = (GridGenerator)target;

        if (GUILayout.Button("Regenerate Grid"))
        {
            gen.GenerateGrid();
            Debug.Log("Grid updated.");
        }
    }
}

#endif

public class GridGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] Tilemap _obstaclesTilemap;

    bool[,] _walkableGrid;
    int _gridWidth;
    int _gridHeight;

    internal void GenerateGrid()
    {
        BoundsInt bounds = _obstaclesTilemap.cellBounds;
        _gridWidth = bounds.size.x;
        _gridHeight = bounds.size.y;

        _walkableGrid = new bool[_gridWidth, _gridHeight];

        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                Vector3Int cellPos = new Vector3Int(bounds.x + x, bounds.y + y, 0);

                _walkableGrid[x, y] = !_obstaclesTilemap.HasTile(cellPos);
            }
        }
    }

    public Vector3 CellToWorld(Vector2Int cell) => _obstaclesTilemap.CellToWorld((Vector3Int)cell) + (Vector3)_obstaclesTilemap.cellSize / 2f;

    public Vector2Int WorldToCell(Vector3 worldPos) => (Vector2Int)_obstaclesTilemap.WorldToCell(worldPos);

    public bool IsWalkable(Vector2Int cell)
    {
        int x = cell.x - _obstaclesTilemap.cellBounds.x;
        int y = cell.y - _obstaclesTilemap.cellBounds.y;

        if (x < 0 || y < 0 || x >= _gridWidth || y >= _gridHeight) return false;
        return _walkableGrid[x, y];
    }

    private void OnDrawGizmos()
    {
        if (_walkableGrid == null) return;

        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                Gizmos.color = _walkableGrid[x, y] ? Color.green : Color.red;
                Vector3 worldPos = CellToWorld(new Vector2Int(x + _obstaclesTilemap.cellBounds.x, y + _obstaclesTilemap.cellBounds.y));
                Gizmos.DrawWireCube(worldPos, _obstaclesTilemap.cellSize);
            }
        }
    }
}
