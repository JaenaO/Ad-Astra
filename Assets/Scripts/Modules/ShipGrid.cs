using UnityEngine;

namespace Modules
{
  /// <summary>
  /// Data model for ship module placement on a centered 2D grid.
  /// Grid positions are relative to center: (0,0) is the middle cell.
  /// </summary>
  public sealed class ShipGrid
  {
    private static readonly Vector2Int[] CardinalDirections =
    {
      Vector2Int.up,
      Vector2Int.down,
      Vector2Int.left,
      Vector2Int.right,
    };

    private readonly GameObject[,] modules;
    private readonly Vector2Int centerIndex;

    public int Width { get; }
    public int Height { get; }

    public ShipGrid(int width, int height)
    {
      Width = Mathf.Max(1, width);
      Height = Mathf.Max(1, height);
      centerIndex = new Vector2Int(Width / 2, Height / 2);
      modules = new GameObject[Width, Height];
    }

    public Vector2Int IndexToGridPosition(int x, int y)
    {
      return new Vector2Int(x - centerIndex.x, y - centerIndex.y);
    }

    public bool IsOccupiedIndex(int x, int y)
    {
      if (x < 0 || x >= Width || y < 0 || y >= Height)
        return false;

      return modules[x, y] != null;
    }

    public bool IsInBounds(Vector2Int gridPosition)
    {
      return TryGetIndex(gridPosition, out _);
    }

    public bool IsOccupied(Vector2Int gridPosition)
    {
      return TryGetIndex(gridPosition, out var index) && modules[index.x, index.y] != null;
    }

    public GameObject GetModule(Vector2Int gridPosition)
    {
      return TryGetIndex(gridPosition, out var index) ? modules[index.x, index.y] : null;
    }

    public bool TrySetModule(Vector2Int gridPosition, GameObject moduleObject)
    {
      if (moduleObject == null || !TryGetIndex(gridPosition, out var index))
        return false;

      if (modules[index.x, index.y] != null)
        return false;

      modules[index.x, index.y] = moduleObject;
      return true;
    }

    public bool TryClearModule(Vector2Int gridPosition, out GameObject moduleObject)
    {
      moduleObject = null;
      if (!TryGetIndex(gridPosition, out var index))
          return false;

      moduleObject = modules[index.x, index.y];
      if (moduleObject == null)
        return false;

      modules[index.x, index.y] = null;
      return true;
    }

    public bool HasAnyModule()
    {
      for (int x = 0; x < Width; x++)
      {
        for (int y = 0; y < Height; y++)
        {
          if (modules[x, y] != null)
            return true;
        }
      }

      return false;
    }

    public bool HasAdjacentModule(Vector2Int gridPosition)
    {
      if (!TryGetIndex(gridPosition, out var index))
        return false;

      foreach (var direction in CardinalDirections)
      {
        var neighbor = index + direction;
        if (neighbor.x < 0 || neighbor.x >= Width || neighbor.y < 0 || neighbor.y >= Height)
          continue;

        if (modules[neighbor.x, neighbor.y] != null)
          return true;
      }

      return false;
    }

    private bool TryGetIndex(Vector2Int gridPosition, out Vector2Int index)
    {
      index = centerIndex + gridPosition;
      return index.x >= 0
        && index.x < Width
        && index.y >= 0
        && index.y < Height;
    }
  }
}
