using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class WalkableGrid : MonoBehaviour
{
    Pathfinding _walkableGrid;
    public static WalkableGrid Instance { get; private set; }
    public Pathfinding Walkable { get { return _walkableGrid; } }
    
    [Header("Tilemaps")]
    [SerializeField] List<Tilemap> _obstacleTilemaps;

    private void Awake()
    {
        Instance = this;
        GenerateGrid();
    }

    internal void GenerateGrid()
    {
        List<Tilemap> tilemaps = new List<Tilemap>();
        BoundsInt bounds = new BoundsInt();

        bool firstTilemap = true;

        // Add tilemaps to list and set the bounds
        foreach (Tilemap currentTilemap in _obstacleTilemaps)
        {
            tilemaps.Add(currentTilemap);

            if (firstTilemap)
            {
                bounds = currentTilemap.cellBounds;
                firstTilemap = false;
            }
            else
            {
                // Expand tilemap limits
                bounds.xMin = Mathf.Min(bounds.xMin, currentTilemap.cellBounds.xMin);
                bounds.yMin = Mathf.Min(bounds.yMin, currentTilemap.cellBounds.yMin);
                bounds.xMax = Mathf.Max(bounds.xMax, currentTilemap.cellBounds.xMax);
                bounds.yMax = Mathf.Max(bounds.yMax, currentTilemap.cellBounds.yMax);
            }
        }

        if (tilemaps.Count == 0)
        {
            Debug.LogWarning("Couldn't find any tilemap component in object's children.");
        }

        // Crear Grid con los límites calculados
        int width = bounds.size.x;
        int height = bounds.size.y;
        float cellSize = 1f;
        Vector3 origin = new Vector3(bounds.xMin, bounds.yMin, 0);

        _walkableGrid = new Pathfinding(width, height, cellSize, origin);
        Debug.Log($"<b><color=#BF8AFF>Created grid with expected width: {width}, height: {height}, origin: {origin} || output width: {_walkableGrid.GetGrid().Width}, output height: {_walkableGrid.GetGrid().Height}</color></b>");

        // --- Asignar walkability en función de los tilemaps ---
        var grid = _walkableGrid.GetGrid();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Node node = grid.GetGridObject(x, y);
                Vector3Int cellPos = new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0);

                bool isBlocked = false;
                foreach (var tilemap in _obstacleTilemaps)
                {
                    if (tilemap != null && tilemap.HasTile(cellPos))
                    {
                        isBlocked = true;
                        break;
                    }
                }

                node.IsWalkable = !isBlocked;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_walkableGrid == null || _walkableGrid.GetGrid() == null) return;

        var grid = _walkableGrid.GetGrid();

        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                Node node = grid.GetGridObject(x, y);
                if (node == null) continue;

                Gizmos.color = node.IsWalkable ? Color.green : Color.red;

                // Obtener la posición central de la celda
                Vector3 worldPos = grid.GetWorldPos(x, y) + new Vector3(grid.CellSize, grid.CellSize) * 0.5f;
                Gizmos.DrawWireCube(worldPos, new Vector3(grid.CellSize, grid.CellSize, -1));
            }
        }
    }

}
