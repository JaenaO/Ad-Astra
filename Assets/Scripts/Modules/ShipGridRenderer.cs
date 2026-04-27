using UnityEngine;

namespace Modules
{
    internal sealed class ShipGridRenderer
    {
        private readonly Transform parent;
        private readonly float depthOffset;
        private readonly int sortingOrder;

        private Transform root;
        private Sprite cellSprite;
        private SpriteRenderer[,] cellRenderers;

        public ShipGridRenderer(Transform parent, float depthOffset, int sortingOrder)
        {
            this.parent = parent;
            this.depthOffset = depthOffset;
            this.sortingOrder = sortingOrder;
        }

        public void Rebuild(ShipGrid shipGrid, float cellSize, Vector2 gridOriginOffset, Color emptyCellColor)
        {
            if (root != null)
                Object.Destroy(root.gameObject);

            if (cellSprite == null)
                cellSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);

            root = new GameObject("GridVisuals").transform;
            root.SetParent(parent, false);

            cellRenderers = new SpriteRenderer[shipGrid.Width, shipGrid.Height];
            float visualScale = Mathf.Max(0.05f, cellSize * 0.95f);

            for (int x = 0; x < shipGrid.Width; x++)
            {
                for (int y = 0; y < shipGrid.Height; y++)
                {
                    Vector2Int gridPosition = shipGrid.IndexToGridPosition(x, y);

                    GameObject cell = new GameObject($"Cell_{x}_{y}");
                    cell.transform.SetParent(root, false);
                    cell.transform.localPosition = GridToVisualLocalPosition(gridPosition, cellSize, gridOriginOffset);
                    cell.transform.localRotation = Quaternion.identity;
                    cell.transform.localScale = new Vector3(visualScale, visualScale, 1f);

                    var renderer = cell.AddComponent<SpriteRenderer>();
                    renderer.sprite = cellSprite;
                    renderer.color = emptyCellColor;
                    renderer.sortingOrder = sortingOrder;

                    cellRenderers[x, y] = renderer;
                }
            }
        }

        public void Refresh(ShipGrid shipGrid, Color emptyCellColor, Color occupiedCellColor)
        {
            if (shipGrid == null || cellRenderers == null)
                return;

            for (int x = 0; x < shipGrid.Width; x++)
            {
                for (int y = 0; y < shipGrid.Height; y++)
                {
                    SpriteRenderer renderer = cellRenderers[x, y];
                    if (renderer == null)
                        continue;

                    bool occupied = shipGrid.IsOccupiedIndex(x, y);
                    renderer.color = occupied ? occupiedCellColor : emptyCellColor;
                }
            }
        }

        public void SetVisible(bool isVisible)
        {
            if (root == null)
                return;

            root.gameObject.SetActive(isVisible);
        }

        private Vector3 GridToVisualLocalPosition(Vector2Int gridPosition, float cellSize, Vector2 gridOriginOffset)
        {
            float activeCellSize = Mathf.Max(0.1f, cellSize);
            float x = gridPosition.x * activeCellSize + gridOriginOffset.x;
            float y = gridPosition.y * activeCellSize + gridOriginOffset.y;
            return new Vector3(x, y, depthOffset);
        }
    }
}
