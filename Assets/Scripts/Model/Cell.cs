using UnityEngine;

public class Cell
{
    public Pos Pos;
    public bool HasTile => Tile != null;
    public Tile Tile { get; private set; }

    private Cell _upCell;
    private Cell _downCell;
    private Cell _leftCell;
    private Cell _rightCell;

    public Cell(Pos pos)
    {
        Pos = pos;

        Tile = null;

        _upCell = null;
        _downCell = null;
        _leftCell = null;
        _rightCell = null;
    }

    public void SetConnectingCells(Cell upCell, Cell downCell, Cell leftCell, Cell rightCell)
    {
        _upCell = upCell;
        _downCell = downCell;
        _leftCell = leftCell;
        _rightCell = rightCell;
    }

    public void AddTile(Tile tile)
    {
        Tile = tile;
    }

    public void RemoveTile()
    {
        Tile = null;
    }

    public Cell CellInDir(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:
                return _upCell;
            case Direction.Down:
                return _downCell;
            case Direction.Left:
                return _leftCell;
            case Direction.Right:
                return _rightCell;
            default:
                Debug.Log("Cell: get cell by wrong direction");
                return null;
        }
    }
}
