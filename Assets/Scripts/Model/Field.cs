using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Field
{
    private static Cell[,] _cells;

    public static Action<Pos> OnAddTile;
    public static Action<Pos, Pos> OnMoveTile;
    public static Action<Pos, int> OnSetTileLevel;
    public static Action<Pos> OnDestroyTile;
    public static Action<Pos, Pos, int> OnMatch;
    public static Action OnWin;
    public static Action OnLoss;

    public static int SizeX { get; private set; }
    public static int SizeY { get; private set; }

    private static bool _canMove;

    public static void Load(int p_x, int p_y)
    {
        SizeX = p_x;
        SizeY = p_y;

        _cells = new Cell[p_x, p_y];
        for (int y = 0; y < p_y; ++y)
            for (int x = 0; x < p_x; ++x)
                _cells[x, y] = new Cell(new Pos(x, y));

        _canMove = true;

        for (int y = 0; y < p_y; ++y)
        {
            for (int x = 0; x < p_x; ++x)
            {
                _cells[x, y].SetConnectingCells(
                    (y + 1 < p_y) ? _cells[x, y + 1] : null,
                    (y - 1 >= 0) ? _cells[x, y - 1] : null,
                    (x - 1 >= 0) ? _cells[x - 1, y] : null,
                    (x + 1 < p_x) ? _cells[x + 1, y] : null);
            }
        }
    }

    public static void AddRandomTile()
    {
        var freeCells = FreeCells();

        var CellToAdd = freeCells[Random.Range(0, freeCells.Count)];

        var newTile = new Tile(Random.Range(1, 3));
        CellToAdd.AddTile(newTile);
        OnAddTile.Invoke(CellToAdd.Pos);
        OnSetTileLevel(CellToAdd.Pos, newTile.Level);

        Debug.Log("RandomTile added");
    }

    private static List<Cell> FreeCells()
    {
        var freeCells = new List<Cell>();
        foreach (var cell in _cells)
        {
            if (!cell.HasTile)
                freeCells.Add(cell);
        }

        return freeCells;
    }


    public static void MoveTilesInDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.Left:
                MoveTilesLeft();
                break;
            case Direction.Right:
                MoveTilesRight();
                break;
            case Direction.Up:
                MoveTilesUp();
                break;
            case Direction.Down:
                MoveTilesDown();
                break;
            default:
                return;
        }

        if (_canMove)
        {
            AddRandomTile();
            _canMove = false;
        }

        foreach (var cell in _cells)
        {
            if (!cell.HasTile) return;
        }

        CheckLoss();
    }

    private static void MoveTilesLeft()
    {
        for (int y = 0; y < SizeY; ++y)
        {
            for (int n = 0; n < SizeX; ++n)
            {
                for (int x = 1; x < SizeX; ++x)
                {
                    TryMoveTileInDir(_cells[x, y], Direction.Left);
                }
            }

            CancelAlredyMatches();
        }
    }

    private static void MoveTilesRight()
    {
        for (int y = 0; y < SizeY; ++y)
        {
            for (int n = 0; n < SizeX; ++n)
            {
                for (int x = SizeX - 2; x >= 0; --x)
                {
                    TryMoveTileInDir(_cells[x, y], Direction.Right);
                }
            }

            CancelAlredyMatches();
        }
    }

    private static void MoveTilesUp()
    {
        for (int x = 0; x < SizeX; ++x)
        {
            for (int n = 0; n < SizeY; ++n)
            {
                for (int y = SizeY - 2; y >= 0; --y)
                {        
                    TryMoveTileInDir(_cells[x, y], Direction.Up);          
                }
            }

            CancelAlredyMatches();
        }
    }

    private static void MoveTilesDown()
    {
        for (int x = 0; x < SizeX; ++x)
        {
            for (int n = 0; n < SizeY; ++n)
            {
                for (int y = 1; y < SizeY; ++y)
                {
                    TryMoveTileInDir(_cells[x, y], Direction.Down); 
                }
            }

            CancelAlredyMatches();
        }
    }

    private static void CancelAlredyMatches()
    {
        foreach (var cell in _cells)
            if (cell.HasTile) cell.Tile.IsAlredyMatched = false;
    }

    private static void TryMoveTileInDir(Cell fromCell, Direction dir)
    {
        if (!fromCell.HasTile) 
        {
            //Debug.Log($"Model.Move: fromCell ({fromCell.Pos.X}, {fromCell.Pos.Y}) dont have tile");
            return;
        }

        var toCell = fromCell.CellInDir(dir);
        if (toCell == null)
        {
            Debug.Log($"Model.Move: ToCell ({toCell.Pos.X}, {toCell.Pos.Y}) == null");
            return;
        }


        if (!toCell.HasTile)
        {
            Debug.Log($"Model: Try move tile from ({fromCell.Pos.X}, {fromCell.Pos.Y}) to ({toCell.Pos.X}, {toCell.Pos.Y})");
            toCell.AddTile(fromCell.Tile);
            fromCell.RemoveTile();
            OnMoveTile.Invoke(fromCell.Pos, toCell.Pos);
            _canMove = true;
        }
        else if (toCell.HasTile && fromCell.Tile.Level == toCell.Tile.Level
            && !fromCell.Tile.IsAlredyMatched && !toCell.Tile.IsAlredyMatched)
        {
            Debug.Log($"Model: Try up leel from ({fromCell.Pos.X}, {fromCell.Pos.Y}) to ({toCell.Pos.X}, {toCell.Pos.Y})");
            toCell.Tile.UpLevel();

            OnMatch.Invoke(fromCell.Pos, toCell.Pos, toCell.Tile.Level);
            //OnMoveTile.Invoke(fromCell.Pos, toCell.Pos);
            //OnSetTileLevel.Invoke(toCell.Pos, toCell.Tile.Level);
            fromCell.RemoveTile();
            //OnDestroyTile(fromCell.Pos);
            toCell.Tile.IsAlredyMatched = true;
            _canMove = true;
        }
    }
    private static void CheckLoss()
    {
        foreach (var cell in _cells)
            if (!cell.HasTile) return;

        foreach (var cell in _cells)
        {
            for (int i = 1; i < 5; ++i)
            {
                if (cell.CellInDir((Direction)i) != null &&
                    cell.CellInDir((Direction)i).Tile.Level == cell.Tile.Level)
                    return;
            }
        }

        OnLoss.Invoke();
    }
}
