public class Node
{
    Grid<Node> _grid;
    int x, y;
    int _gCost, _hCost, _fCost;
    bool _isWalkable;
    Node _cameFromNode;

    public int X => x;
    public int Y => y;
    public int GCost { get { return _gCost; } set { _gCost = value; } }
    public int HCost { get { return _hCost; } set { _hCost = value; } }
    public int FCost { get { return _fCost; } }
    public bool IsWalkable { get { return _isWalkable; } set { _isWalkable = value; } }
    public Node CameFromNode { get { return _cameFromNode; } set { _cameFromNode = value; } }

    public Node(Grid<Node> grid, int x, int y)
    {
        _grid = grid;
        this.x = x;
        this.y = y;
        _isWalkable = true;
    }

    public override string ToString()
    {
        return x + ", " + y;
    }

    public void CalculateFCost() { _fCost = _gCost + _hCost; }
}
