using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modules
{
    public class ShipModuleBuilder : MonoBehaviour
    {
        private static readonly Vector2Int[] CardinalDirections =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
        };

        private const int GridScale = 2;
        private const int VerticalGridTiles = 10;
        private const int HorizontalGridTiles = 5;

        private readonly Vector2Int CenterIndex = new(HorizontalGridTiles / 2, VerticalGridTiles / 2);

        private readonly GameObject[,] ModuleGrid = new GameObject[HorizontalGridTiles, VerticalGridTiles];
        private Dictionary<string, ModuleDefinition> ModuleStorage;

        [SerializeField] private Vector2 gridOriginOffset;

        [NonSerialized]
        public int CurrentWeight = 0;
        [NonSerialized]
        public int MaxWeight = 0;
        [NonSerialized]
        public int CurrentEngines = 0;
        [NonSerialized]
        public int MaxEngines = 2;
        
        [Header("Debug Grid")]
        [SerializeField] private bool showGrid = true;
        [SerializeField] private Color emptyCellColor = new(0.2f, 0.8f, 1f, 0.25f);
        [SerializeField] private Color occupiedCellColor = new(1f, 0.75f, 0.2f, 0.45f);
        
        private void OnDrawGizmos()
        {
            if (!showGrid)
                return;

            for (int x = 0; x < HorizontalGridTiles; x++)
            {
                for (int y = 0; y < VerticalGridTiles; y++)
                {
                    Vector2Int gridPosition = new(
                        x - CenterIndex.x,
                        y - CenterIndex.y);

                    Vector3 localCenter = new(
                        gridPosition.x * GridScale,
                        gridPosition.y * GridScale,
                        0f);

                    Vector3 worldCenter = transform.TransformPoint(localCenter);
                    bool occupied = ModuleGrid[x, y] != null;

                    Gizmos.color = occupied ? occupiedCellColor : emptyCellColor;
                    Gizmos.DrawWireCube(worldCenter, new Vector3(GridScale, GridScale, 0.05f));
                }
            }
        }

        /// <summary>
        /// Converts a grid cell offset into local space.
        /// </summary>
        private Vector3 GridToLocalPosition(Vector2Int gridPosition)
        {
            return new Vector3(
                gridPosition.x * GridScale + gridOriginOffset.x,
                gridPosition.y * GridScale + gridOriginOffset.y,
                0f
            );
        }

        /// <summary>
        /// Converts a world-space position into a relative ship grid cell.
        /// </summary>
        public bool TryWorldToGridPosition(Vector3 worldPosition, out Vector2Int gridPosition)
        {
            var local = transform.InverseTransformPoint(worldPosition);
            local.x -= gridOriginOffset.x;
            local.y -= gridOriginOffset.y;

            gridPosition = new Vector2Int(
                Mathf.RoundToInt(local.x / GridScale),
                Mathf.RoundToInt(local.y / GridScale)
            );

            return TryGetGridIndex(gridPosition, out _);
        }

        /// <summary>
        /// Converts a relative grid position into the array index used by ModuleGrid.
        /// </summary>
        private bool TryGetGridIndex(Vector2Int gridPosition, out Vector2Int gridIndex)
        {
            gridIndex = CenterIndex + gridPosition;
            return gridIndex.x >= 0
                && gridIndex.x < HorizontalGridTiles
                && gridIndex.y >= 0
                && gridIndex.y < VerticalGridTiles;
        }

        /// <summary>
        /// Returns the module currently stored at a grid position.
        /// </summary>
        public GameObject GetModuleAt(Vector2Int gridPosition)
        {
            return TryGetGridIndex(gridPosition, out var gridIndex) ? ModuleGrid[gridIndex.x, gridIndex.y] : null;
        }

        /// <summary>
        /// Checks whether a grid position touches the existing ship.
        /// </summary>
        private bool HasAdjacentModule(Vector2Int gridIndex)
        {
            foreach (var direction in CardinalDirections)
            {
                var neighbor = gridIndex + direction;
                if (neighbor.x < 0 || neighbor.x >= HorizontalGridTiles || neighbor.y < 0 || neighbor.y >= VerticalGridTiles)
                    continue;

                if (ModuleGrid[neighbor.x, neighbor.y] != null)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whether any modules remain on the grid.
        /// </summary>
        private bool HasAnyModule()
        {
            for (var x = 0; x < HorizontalGridTiles; x++)
            {
                for (var y = 0; y < VerticalGridTiles; y++)
                {
                    if (ModuleGrid[x, y] != null)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a module definition by id and logs a warning if it is missing.
        /// </summary>
        private bool TryGetModuleDefinition(string moduleId, out ModuleDefinition moduleDefinition)
        {
            if (ModuleStorage != null && ModuleStorage.TryGetValue(moduleId, out moduleDefinition) && moduleDefinition != null)
                return true;

            Debug.LogWarning($"Missing module definition: '{moduleId}'.");
            moduleDefinition = null;
            return false;
        }

        /// <summary>
        /// Applies the module to the grid and updates ship stats.
        /// </summary>
        private GameObject CreateModule(ModuleDefinition moduleDefinition, Vector2Int gridPosition)
        {
            if (!TryGetGridIndex(gridPosition, out var gridIndex))
            {
                Debug.LogWarning($"Grid position {gridPosition} is outside the build area.");
                return null;
            }

            var newModule = Instantiate(moduleDefinition.prefab, GridToLocalPosition(gridPosition), Quaternion.identity, transform);
            ModuleGrid[gridIndex.x, gridIndex.y] = newModule;

            CurrentWeight += moduleDefinition.weightCost;
            MaxWeight += moduleDefinition.maxWeightBonus;

            if (moduleDefinition.category == ModuleCategory.Engine)
                CurrentEngines += 1;

            var instance = newModule.AddComponent<ModuleInstance>();
            instance.definition = moduleDefinition;
            instance.Activate();

            return newModule;
        }

        /// <summary>
        /// Places a module without validation. Use TryPlaceModule for player-driven placement.
        /// </summary>
        public GameObject PlaceModule(ModuleDefinition moduleDefinition, Vector2Int gridPosition)
        {
            return CreateModule(moduleDefinition, gridPosition);
        }

        /// <summary>
        /// Validates whether the player can place a module on the current ship.
        /// </summary>
        public bool CanPlaceModule(ModuleDefinition moduleDefinition, Vector2Int gridPosition)
        {
            if (moduleDefinition == null || moduleDefinition.prefab == null)
                return false;

            if (!TryGetGridIndex(gridPosition, out var gridIndex))
                return false;

            if (ModuleGrid[gridIndex.x, gridIndex.y] != null)
                return false;

            if (CurrentWeight + moduleDefinition.weightCost > MaxWeight)
                return false;

            if (moduleDefinition.category == ModuleCategory.Engine && CurrentEngines + 1 > MaxEngines)
                return false;

            if (HasAnyModule() && !HasAdjacentModule(gridIndex))
                return false;

            if (GameManager.Instance == null)
                return false;

            var costs = moduleDefinition.costs ?? Array.Empty<ResourceCost>();
            foreach (var cost in costs)
            {
                if (GameManager.Instance.GetStock(cost.tier) < cost.amount)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Places a module if the ship rules and resource checks allow it.
        /// </summary>
        public bool TryPlaceModule(ModuleDefinition moduleDefinition, Vector2Int gridPosition)
        {
            if (!CanPlaceModule(moduleDefinition, gridPosition))
                return false;

            var costs = moduleDefinition.costs ?? Array.Empty<ResourceCost>();
            foreach (var cost in costs)
                GameManager.Instance.Spend(cost.tier, cost.amount);

            CreateModule(moduleDefinition, gridPosition);
            return true;
        }

        /// <summary>
        /// Removes a module from the grid and updates ship stats.
        /// </summary>
        public bool TryRemoveModule(Vector2Int gridPosition)
        {
            if (!TryGetGridIndex(gridPosition, out var gridIndex))
                return false;

            var moduleObject = ModuleGrid[gridIndex.x, gridIndex.y];
            if (moduleObject == null)
                return false;

            var instance = moduleObject.GetComponent<ModuleInstance>();
            if (instance == null || instance.definition == null)
                return false;

            ModuleGrid[gridIndex.x, gridIndex.y] = null;
            CurrentWeight = Mathf.Max(0, CurrentWeight - instance.definition.weightCost);
            MaxWeight = Mathf.Max(0, MaxWeight - instance.definition.maxWeightBonus);

            if (instance.definition.category == ModuleCategory.Engine)
                CurrentEngines = Mathf.Max(0, CurrentEngines - 1);

            Destroy(moduleObject);
            return true;
        }

        /// <summary>
        /// Moves a module to another valid grid position without changing its stats.
        /// </summary>
        public bool TryMoveModule(Vector2Int fromGridPosition, Vector2Int toGridPosition)
        {
            if (fromGridPosition == toGridPosition)
                return true;

            if (!TryGetGridIndex(fromGridPosition, out var fromGridIndex))
                return false;

            if (!TryGetGridIndex(toGridPosition, out var toGridIndex))
                return false;

            var moduleObject = ModuleGrid[fromGridIndex.x, fromGridIndex.y];
            if (moduleObject == null || ModuleGrid[toGridIndex.x, toGridIndex.y] != null)
                return false;

            ModuleGrid[fromGridIndex.x, fromGridIndex.y] = null;

            if (HasAnyModule() && !HasAdjacentModule(toGridIndex))
            {
                ModuleGrid[fromGridIndex.x, fromGridIndex.y] = moduleObject;
                return false;
            }

            ModuleGrid[toGridIndex.x, toGridIndex.y] = moduleObject;
            moduleObject.transform.localPosition = GridToLocalPosition(toGridPosition);
            return true;
        }

        private void Awake()
        {
            ModuleStorage = new Dictionary<string, ModuleDefinition>();

            foreach (var module in Resources.LoadAll<ModuleDefinition>("ModuleData"))
            {
                if (module == null)
                    continue;

                if (string.IsNullOrWhiteSpace(module.id))
                {
                    Debug.LogWarning($"Module asset '{module.name}' has no id.");
                    continue;
                }

                if (module.prefab == null)
                {
                    Debug.LogWarning($"Module '{module.id}' has no prefab assigned.");
                    continue;
                }

                if (ModuleStorage.ContainsKey(module.id))
                {
                    Debug.LogWarning($"Duplicate module id found: '{module.id}'.");
                    continue;
                }

                ModuleStorage.Add(module.id, module);
            }

            Debug.Log($"Loaded {ModuleStorage.Count} module definitions.");
        }

        private void Start()
        {
            // The starter ship is fixed so the building scene begins with a clear shape.
            if (TryGetModuleDefinition("BASIC_CLAW", out var claw))
                PlaceModule(claw, Vector2Int.up);

            if (TryGetModuleDefinition("BASIC_CORE", out var core))
                PlaceModule(core, Vector2Int.zero);

            if (TryGetModuleDefinition("BASIC_ENGINE", out var engineLeft))
                PlaceModule(engineLeft, Vector2Int.left);

            if (TryGetModuleDefinition("BASIC_ENGINE", out var engineRight))
                PlaceModule(engineRight, Vector2Int.right);
        }
    }
}
