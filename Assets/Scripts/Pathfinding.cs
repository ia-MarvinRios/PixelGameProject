using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    const int MOVE_STRAIGHT_COST = 10;
    const int MOVE_DIAGONAL_COST = 14;
    static readonly Vector3 DEFAULT_ORIGIN = new Vector3(0f, 0f, 0f);

    public static Pathfinding Instance { get; private set; }
    Grid<Node> _grid;
    List<Node> _openList;
    List<Node> _closedList;

    public Pathfinding(int width, int height, float cellSize, Vector3? origin = null)
    {
        Vector3 gridOrigin = origin ?? DEFAULT_ORIGIN;
        Instance = this;
        _grid = new Grid<Node>(width, height, cellSize, gridOrigin, (Grid<Node> g, int x, int y) => new Node(g, x, y));
    }

    public Grid<Node> GetGrid() { return _grid; }

    public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition)
    {
        _grid.GetXY(startWorldPosition, out int startX, out int startY);
        _grid.GetXY(endWorldPosition, out int endX, out int endY);

        //DEBUG
        if (endX < 0 || endY < 0 || endX >= _grid.Width || endY >= _grid.Height) {
            Debug.LogWarning($"<color=orange>Target is outside grid! WorldPos={endWorldPosition}, GridIndex=({endX},{endY})</color>");
            return null;
        }

        List<Node> path = FindPath(startX, startY, endX, endY);

        if (path == null) {
            return null;
        }
        else {
            //MODIFIED
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (Node node in path) {
                Vector3 worldPos = _grid.GetWorldPos(node.X, node.Y) + new Vector3(_grid.CellSize, _grid.CellSize, 0) * 0.5f;
                vectorPath.Add(worldPos);
            }


            return vectorPath;
        }
    }

    public List<Node> FindPath(int startX, int startY, int endX, int endY)
    {
        Node startNode = _grid.GetGridObject(startX, startY);
        Node endNode = _grid.GetGridObject(endX, endY);

        //DEBUG
        if (startNode == null || endNode == null) {
            Debug.LogWarning($"<color=orange>Invalid start or end node: start=({startX},{startY}), end=({endX},{endY})</color>");
            return null;
        }

        _openList = new List<Node> { startNode };
        _closedList = new List<Node>();

        for (int x = 0; x < _grid.Width; x++) {
            for (int y = 0; y < _grid.Height; y++)
            {
                Node node = _grid.GetGridObject(x, y);
                node.GCost = 99999999;
                node.CalculateFCost();
                node.CameFromNode = null;
            }
        }

        startNode.GCost = 0;
        startNode.HCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (_openList.Count > 0)
        {
            Node currentNode = GetLowestFCostNode(_openList);
            if (currentNode == endNode) {
                return CalculatePath(endNode);
            }

            _openList.Remove(currentNode);
            _closedList.Add(currentNode);

            foreach (Node neighbournode in GetNeighbourList(currentNode)) {
                if (_closedList.Contains(neighbournode)) continue;
                if (!neighbournode.IsWalkable) {
                    _closedList.Add(neighbournode);
                    continue;
                }

                int tentativeGCost = currentNode.GCost + CalculateDistanceCost(currentNode, neighbournode);
                if (tentativeGCost < neighbournode.GCost) {
                    neighbournode.CameFromNode = currentNode;
                    neighbournode.GCost = tentativeGCost;
                    neighbournode.HCost = CalculateDistanceCost(neighbournode, endNode);
                    neighbournode.CalculateFCost();

                    if (!_openList.Contains(neighbournode)) {
                        _openList.Add(neighbournode);
                    }
                }
            }
        }

        // Out of nodes on the open list
        return null;

    }

    List<Node> GetNeighbourList(Node currentNode)
    {
        List<Node> neighbourList = new List<Node>();

        if (currentNode.X - 1 >= 0)
        {
            // Left
            neighbourList.Add(GetNode(currentNode.X - 1, currentNode.Y));
            // Left Down
            if (currentNode.Y - 1 >= 0) neighbourList.Add(GetNode(currentNode.X - 1, currentNode.Y - 1));
            // Left Up
            if (currentNode.Y + 1 < _grid.Height) neighbourList.Add(GetNode(currentNode.X - 1, currentNode.Y + 1));
        }
        if (currentNode.X + 1 < _grid.Width)
        {
            // Right
            neighbourList.Add(GetNode(currentNode.X + 1, currentNode.Y));
            // Right Down
            if (currentNode.Y - 1 >= 0) neighbourList.Add(GetNode(currentNode.X + 1, currentNode.Y - 1));
            // Right Up
            if (currentNode.Y + 1 < _grid.Height) neighbourList.Add(GetNode(currentNode.X + 1, currentNode.Y + 1));
        }
        // Down
        if (currentNode.Y - 1 >= 0) neighbourList.Add(GetNode(currentNode.X, currentNode.Y - 1));
        // Up
        if (currentNode.Y + 1 < _grid.Height) neighbourList.Add(GetNode(currentNode.X, currentNode.Y + 1));

        return neighbourList;
    }

    Node GetNode(int x, int y)
    {
        return _grid.GetGridObject(x, y);
    }

    List<Node> CalculatePath(Node endNode)
    {
        List<Node> path = new List<Node>();
        path.Add(endNode);
        Node currentNode = endNode;
        while (currentNode.CameFromNode != null)
        {
            path.Add(currentNode.CameFromNode);
            currentNode = currentNode.CameFromNode;
        }

        path.Reverse();
        return path;
    }

    int CalculateDistanceCost(Node a, Node b)
    {
        int xDistance = Mathf.Abs(a.X - b.X);
        int yDistance = Mathf.Abs(a.Y - b.Y);
        int remaining = Mathf.Abs(xDistance - yDistance);

        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    Node GetLowestFCostNode(List<Node> nodeList)
    {
        Node lowestFCostNode = nodeList[0];
        for (int i = 1; i < nodeList.Count; i++)
        {
            if (nodeList[i].FCost < lowestFCostNode.FCost)
                lowestFCostNode = nodeList[i];
        }

        return lowestFCostNode;
    }
}
