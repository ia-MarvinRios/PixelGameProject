using System;
using UnityEngine;

public class Grid<TGridObject> {

    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs {
        public int x;
        public int y;
    }

    int _width;
    int _height;
    float _cellSize;
    Vector3 _origin = Vector3.zero;
    TGridObject[,] _gridArray;

    public int Width { get { return _width; } private set { _width = value; } }
    public int Height { get { return _height; } private set { _height = value; } }
    public float CellSize { get { return _cellSize; } private set { _cellSize = value; } }

    public Grid(int width, int height, float cellSize, Vector3 originPosition, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject) {
        _width = width;
        _height = height;
        _cellSize = cellSize;
        _origin = originPosition;

        _gridArray = new TGridObject[width, height];

        for (int i = 0; i < _gridArray.GetLength(0); i++) {

            for (int j = 0; j < _gridArray.GetLength(1); j++) {
                _gridArray[i, j] = createGridObject(this, i, j);
            }
        }

    }

    public Vector3 GetWorldPos(int x, int y) {
        return new Vector3(x, y) * _cellSize + _origin;
    }
    public void GetXY(Vector3 worldPos, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPos.x - _origin.x) / _cellSize);
        y = Mathf.FloorToInt((worldPos.y - _origin.y) / _cellSize);
    }
    public void SetGridObject(int x, int y, TGridObject value) {
        if (x >= 0 && y >= 0 && x < _width && y < _height) {
            _gridArray[x, y] = value;
            if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
        }
    }
    public void TriggerGridObjectChanged(int x, int y) {
        if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
    }
    public void SetGridObject(Vector3 worldPos, TGridObject value) {
        GetXY(worldPos, out int x, out int y);
        SetGridObject(x, y, value);
    }
    public TGridObject GetGridObject(int x, int y) {
        if (x >= 0 && y >= 0 && x < _width && y < _height)
            return _gridArray[x, y];
        else
            return default(TGridObject);
    }
    public TGridObject GetGridObject(Vector3 worldPos) {
        GetXY(worldPos, out int x, out int y);
        return GetGridObject(x, y);
    }
}
