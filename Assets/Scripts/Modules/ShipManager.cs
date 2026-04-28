using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modules
{
    public class ShipModuleBuilder : MonoBehaviour
    {
        [Header("Grid Settings")]
        [Min(1)]
        [SerializeField] private int horizontalGridTiles = 9;
        [Min(1)]
        [SerializeField] private int verticalGridTiles = 6;
        [Min(0.1f)]
        [SerializeField] private float cellSize = 2f;
        [SerializeField] private Vector2 gridOriginOffset;

        private ShipGrid shipGrid;
        private Dictionary<string, ModuleDefinition> moduleStorage;
        private ShipGridRenderer gridRenderer;
        private bool isBuilderMode;

        private const float ModuleDepthOffset = 0f;
        private const float GridVisualDepthOffset = 0f;
        private const int GridSortingOrder = 5000;
        private const string ModuleResourcePath = "ModuleData";
        private static readonly Color EmptyCellColor = new(0.2f, 0.8f, 1f, 0.45f);
        private static readonly Color OccupiedCellColor = new(1f, 0.75f, 0.2f, 0.6f);

        [NonSerialized] public int CurrentWeight = 0;
        [NonSerialized] public int MaxWeight = 0;
        [NonSerialized] public int CurrentEngines = 0;

        private Vector3 GridToLocalPosition(Vector2Int gridPosition)
        {
            float activeCellSize = Mathf.Max(0.1f, cellSize);
            float x = gridPosition.x * activeCellSize + gridOriginOffset.x;
            float y = gridPosition.y * activeCellSize + gridOriginOffset.y;
            return new Vector3(x, y, ModuleDepthOffset);
        }

        public bool TryWorldToGridPosition(Vector3 worldPosition, out Vector2Int gridPosition)
        {
            float activeCellSize = Mathf.Max(0.1f, cellSize);
            Vector3 local = transform.InverseTransformPoint(worldPosition);

            float x = local.x - gridOriginOffset.x;
            float y = local.y - gridOriginOffset.y;

            gridPosition = new Vector2Int(
                Mathf.RoundToInt(x / activeCellSize),
                Mathf.RoundToInt(y / activeCellSize));

            return shipGrid != null && shipGrid.IsInBounds(gridPosition);
        }

        public GameObject GetModuleAt(Vector2Int gridPosition)
        {
            return shipGrid?.GetModule(gridPosition);
        }

        public Vector3 GetBuildPlaneNormalWorld()
        {
            return transform.forward;
        }

        public Vector3 GetBuildPlanePointWorld()
        {
            return transform.TransformPoint(GridToLocalPosition(Vector2Int.zero));
        }

        private bool TryGetModuleDefinition(string moduleId, out ModuleDefinition moduleDefinition)
        {
            if (moduleStorage != null && moduleStorage.TryGetValue(moduleId, out moduleDefinition) && moduleDefinition != null)
                return true;

            Debug.LogWarning($"Missing module definition: '{moduleId}'.");
            moduleDefinition = null;
            return false;
        }

        private GameObject CreateModule(ModuleDefinition moduleDefinition, Vector2Int gridPosition)
        {
            if (shipGrid == null || !shipGrid.IsInBounds(gridPosition))
            {
                Debug.LogWarning($"Grid position {gridPosition} is outside the build area.");
                return null;
            }

            if (shipGrid.IsOccupied(gridPosition))
            {
                Debug.LogWarning($"Grid position {gridPosition} is already occupied.");
                return null;
            }

            GameObject newModule = Instantiate(moduleDefinition.prefab, transform);
            newModule.transform.localPosition = GridToLocalPosition(gridPosition);
            newModule.transform.localRotation = Quaternion.identity;
            shipGrid.TrySetModule(gridPosition, newModule);

            CurrentWeight += moduleDefinition.weightCost;
            MaxWeight += moduleDefinition.maxWeightBonus;

            if (moduleDefinition.category == ModuleCategory.Engine)
                CurrentEngines += 1;

            ModuleInstance instance = newModule.AddComponent<ModuleInstance>();
            instance.definition = moduleDefinition;
            instance.enabled = false;

            return newModule;
        }

        public GameObject PlaceModule(ModuleDefinition moduleDefinition, Vector2Int gridPosition)
        {
            GameObject created = CreateModule(moduleDefinition, gridPosition);
            if (created != null)
                SaveLayoutAndRefreshGridVisuals();

            return created;
        }

        public bool CanPlaceModule(ModuleDefinition moduleDefinition, Vector2Int gridPosition)
        {
            if (moduleDefinition == null || moduleDefinition.prefab == null)
                return false;

            if (shipGrid == null || !shipGrid.IsInBounds(gridPosition))
                return false;

            if (shipGrid.IsOccupied(gridPosition))
                return false;

            if (CurrentWeight + moduleDefinition.weightCost > MaxWeight)
                return false;

            if (shipGrid.HasAnyModule() && !shipGrid.HasAdjacentModule(gridPosition))
                return false;

            ResourceCost[] costs = moduleDefinition.costs ?? Array.Empty<ResourceCost>();
            if (GameManager.Instance == null)
                return true;

            foreach (var cost in costs)
            {
                if (GameManager.Instance.GetStock(cost.tier) < cost.amount)
                    return false;
            }

            return true;
        }

        public bool TryPlaceModule(ModuleDefinition moduleDefinition, Vector2Int gridPosition)
        {
            if (!CanPlaceModule(moduleDefinition, gridPosition))
                return false;

            ResourceCost[] costs = moduleDefinition.costs ?? Array.Empty<ResourceCost>();
            if (GameManager.Instance)
            {
                foreach (var cost in costs)
                    GameManager.Instance.Spend(cost.tier, cost.amount);
            }

            GameObject created = CreateModule(moduleDefinition, gridPosition);
            if (created == null)
                return false;

            SaveLayoutAndRefreshGridVisuals();
            return true;
        }

        public bool TryRemoveModule(Vector2Int gridPosition)
        {
            if (shipGrid == null)
                return false;

            GameObject moduleObject = shipGrid.GetModule(gridPosition);
            if (moduleObject == null)
                return false;

            ModuleInstance instance = moduleObject.GetComponent<ModuleInstance>();
            if (instance == null || instance.definition == null)
                return false;

            shipGrid.TryClearModule(gridPosition, out _);

            CurrentWeight = Mathf.Max(0, CurrentWeight - instance.definition.weightCost);
            MaxWeight = Mathf.Max(0, MaxWeight - instance.definition.maxWeightBonus);

            if (instance.definition.category == ModuleCategory.Engine)
                CurrentEngines = Mathf.Max(0, CurrentEngines - 1);

            Destroy(moduleObject);
            SaveLayoutAndRefreshGridVisuals();
            return true;
        }

        public bool TryMoveModule(Vector2Int fromGridPosition, Vector2Int toGridPosition)
        {
            if (fromGridPosition == toGridPosition)
                return true;

            if (shipGrid == null)
                return false;

            if (!shipGrid.IsInBounds(fromGridPosition) || !shipGrid.IsInBounds(toGridPosition))
                return false;

            GameObject moduleObject = shipGrid.GetModule(fromGridPosition);
            if (!moduleObject || shipGrid.IsOccupied(toGridPosition))
                return false;

            if (!shipGrid.TryClearModule(fromGridPosition, out _))
                return false;

            if (shipGrid.HasAnyModule() && !shipGrid.HasAdjacentModule(toGridPosition))
            {
                shipGrid.TrySetModule(fromGridPosition, moduleObject);
                return false;
            }

            if (!shipGrid.TrySetModule(toGridPosition, moduleObject))
            {
                shipGrid.TrySetModule(fromGridPosition, moduleObject);
                return false;
            }

            moduleObject.transform.localPosition = GridToLocalPosition(toGridPosition);
            SaveLayoutAndRefreshGridVisuals();
            return true;
        }

        private List<ModulePlacementSnapshot> CaptureCurrentLayout()
        {
            var layout = new List<ModulePlacementSnapshot>();
            if (shipGrid == null)
                return layout;

            for (int x = 0; x < shipGrid.Width; x++)
            {
                for (int y = 0; y < shipGrid.Height; y++)
                {
                    Vector2Int gridPosition = shipGrid.IndexToGridPosition(x, y);
                    GameObject moduleObject = shipGrid.GetModule(gridPosition);
                    if (!moduleObject)
                        continue;

                    ModuleInstance instance = moduleObject.GetComponent<ModuleInstance>();
                    if (!instance || !instance.definition || string.IsNullOrWhiteSpace(instance.definition.id))
                        continue;

                    layout.Add(new ModulePlacementSnapshot
                    {
                        moduleId = instance.definition.id,
                        gridPosition = gridPosition,
                    });
                }
            }

            return layout;
        }

        private void SaveLayoutToSession()
        {
            ShipBuildSessionState.SavedLayout = CaptureCurrentLayout();
        }

        private void SaveLayoutAndRefreshGridVisuals()
        {
            SaveLayoutToSession();
            RefreshGridVisuals();
        }

        private bool TryRestoreLayoutFromSession()
        {
            List<ModulePlacementSnapshot> layout = ShipBuildSessionState.SavedLayout;
            if (layout == null || layout.Count == 0)
                return false;

            foreach (var snapshot in layout)
            {
                    if (!TryGetModuleDefinition(snapshot.moduleId, out ModuleDefinition moduleDefinition))
                    continue;

                CreateModule(moduleDefinition, snapshot.gridPosition);
            }

            return true;
        }

        private void SpawnStarterShip()
        {
            if (TryGetModuleDefinition("BASIC_CLAW", out ModuleDefinition claw))
                PlaceModule(claw, Vector2Int.up);

            if (TryGetModuleDefinition("BASIC_CORE", out ModuleDefinition core))
                PlaceModule(core, Vector2Int.zero);

            if (TryGetModuleDefinition("BASIC_ENGINE", out ModuleDefinition engineLeft))
                PlaceModule(engineLeft, Vector2Int.left);

            if (TryGetModuleDefinition("BASIC_ENGINE", out ModuleDefinition engineRight))
                PlaceModule(engineRight, Vector2Int.right);
        }

        private void OnDisable()
        {
            StopBuilderRuntimeBehaviors();
            SaveLayoutToSession();
        }

        private void OnEnable()
        {
            if (!isBuilderMode)
            {
                SetGridVisibility(false);
                return;
            }

            ClearAsteroidsFromBuildScene();
            SetGridVisibility(true);
            RefreshGridVisuals();
        }

        private void StopBuilderRuntimeBehaviors()
        {
            ModuleInstance[] moduleInstances = GetComponentsInChildren<ModuleInstance>(true);
            foreach (var moduleInstance in moduleInstances)
            {
                moduleInstance.Deactivate();
                moduleInstance.enabled = false;
            }
        }

        private void ClearAsteroidsFromBuildScene()
        {
            asteroid[] asteroids = FindObjectsByType<asteroid>(FindObjectsInactive.Exclude);
            foreach (var asteroidObject in asteroids)
            {
                if (asteroidObject)
                    Destroy(asteroidObject.gameObject);
            }
        }

        private void Awake()
        {
            horizontalGridTiles = Mathf.Max(1, horizontalGridTiles);
            verticalGridTiles = Mathf.Max(1, verticalGridTiles);
            cellSize = Mathf.Max(0.1f, cellSize);
            isBuilderMode = GetComponent<global::BuilderController>() != null;

            shipGrid = new ShipGrid(horizontalGridTiles, verticalGridTiles);
            moduleStorage = new Dictionary<string, ModuleDefinition>();
            gridRenderer = new ShipGridRenderer(transform, GridVisualDepthOffset, GridSortingOrder);

            foreach (var module in ModuleCatalog.LoadModules(ModuleResourcePath))
            {
                if (!module)
                    continue;

                if (string.IsNullOrWhiteSpace(module.id))
                {
                    Debug.LogWarning($"Module asset '{module.name}' has no id.");
                    continue;
                }

                if (!module.prefab)
                {
                    Debug.LogWarning($"Module '{module.id}' has no prefab assigned.");
                    continue;
                }

                if (moduleStorage.ContainsKey(module.id))
                {
                    Debug.LogWarning($"Duplicate module id found: '{module.id}'.");
                    continue;
                }

                moduleStorage.Add(module.id, module);
            }

            if (isBuilderMode)
                gridRenderer.Rebuild(shipGrid, cellSize, gridOriginOffset, EmptyCellColor);

            Debug.Log($"Loaded {moduleStorage.Count} module definitions.");
        }

        private void Start()
        {
            if (!TryRestoreLayoutFromSession())
                SpawnStarterShip();

            SetGridVisibility(isBuilderMode);
            RefreshGridVisuals();
        }

        private void RefreshGridVisuals()
        {
            if (shipGrid == null || gridRenderer == null)
                return;

            if (!isBuilderMode)
            {
                SetGridVisibility(false);
                return;
            }

            gridRenderer.Refresh(shipGrid, EmptyCellColor, OccupiedCellColor);
        }

        private void SetGridVisibility(bool isVisible)
        {
            gridRenderer?.SetVisible(isVisible);
        }
    }
}
