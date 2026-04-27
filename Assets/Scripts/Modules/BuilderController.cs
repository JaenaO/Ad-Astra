using Modules;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuilderController : MonoBehaviour
{
    private static readonly ModuleCategory[] BuildableCategories =
    {
        ModuleCategory.Generator,
        ModuleCategory.Converter,
        ModuleCategory.Engine,
        ModuleCategory.Claw,
    };

    [Header("References")]
    [SerializeField] private ShipModuleBuilder shipModuleBuilder;
    [SerializeField] private UIController uiController;
    [SerializeField] private Camera buildCamera;

    [Header("Controls")]
    [SerializeField] private KeyCode moveKey = KeyCode.M;
    [SerializeField] private KeyCode nextModuleKey = KeyCode.E;
    [SerializeField] private KeyCode previousModuleKey = KeyCode.Q;

    [Header("No-UI Fallback")]
    [SerializeField] private bool enableKeyboardModuleSelection = true;
    [SerializeField] private string moduleResourcePath = "ModuleData";

    private ModuleDefinition selectedModule;
    private ModuleDefinition[] fallbackModules = System.Array.Empty<ModuleDefinition>();
    private int selectedModuleIndex = -1;

    private bool moveModeActive;
    private bool hasMoveSource;
    private Vector2Int moveSourceCell;

    private void Awake()
    {
        if (shipModuleBuilder == null)
            shipModuleBuilder = GetComponent<ShipModuleBuilder>();

        if (buildCamera == null)
            buildCamera = Camera.main;

        LoadFallbackModules();
    }

    private void OnEnable()
    {
        if (uiController != null)
            uiController.OnModuleSelected += HandleModuleSelected;
    }

    private void OnDisable()
    {
        if (uiController != null)
            uiController.OnModuleSelected -= HandleModuleSelected;
    }

    private void Update()
    {
        if (shipModuleBuilder == null)
            return;

        HandleModuleSelectionInput();

        if (Input.GetKeyDown(moveKey))
        {
            moveModeActive = true;
            hasMoveSource = false;
            Debug.Log("Move mode enabled. Click source module, then destination.");
        }

        if (Input.GetMouseButtonDown(1))
        {
            TryRemoveAtCursor();
            return;
        }

        if (Input.GetMouseButtonDown(0))
            HandlePrimaryClick();
    }

    private void LoadFallbackModules()
    {
        fallbackModules = LoadBuildableModules();

        if (fallbackModules.Length == 0)
        {
            Debug.LogWarning($"No modules found at Resources/{moduleResourcePath}.");
            return;
        }

        if (selectedModule == null)
            SelectModuleByIndex(0);
    }

    private ModuleDefinition[] LoadBuildableModules()
    {
        var loadedModules = Resources.LoadAll<ModuleDefinition>(moduleResourcePath);
        var buildableModules = new System.Collections.Generic.List<ModuleDefinition>();

        foreach (var module in loadedModules)
        {
            if (module == null || module.prefab == null)
                continue;

            if (string.Equals(module.id, "BASIC_CORE", System.StringComparison.OrdinalIgnoreCase))
                continue;

            if (string.Equals(module.id, "BASIC_BLOCK", System.StringComparison.OrdinalIgnoreCase))
            {
                buildableModules.Add(module);
                continue;
            }

            bool categoryAllowed = false;
            foreach (var allowedCategory in BuildableCategories)
            {
                if (module.category != allowedCategory)
                    continue;

                categoryAllowed = true;
                break;
            }

            if (!categoryAllowed)
                continue;

            buildableModules.Add(module);
        }

        return buildableModules.ToArray();
    }

    private void HandleModuleSelectionInput()
    {
        if (!enableKeyboardModuleSelection || fallbackModules.Length == 0)
            return;

        if (Input.GetKeyDown(nextModuleKey))
            SelectModuleByIndex((selectedModuleIndex + 1) % fallbackModules.Length);

        if (Input.GetKeyDown(previousModuleKey))
            SelectModuleByIndex((selectedModuleIndex - 1 + fallbackModules.Length) % fallbackModules.Length);

        for (int i = 0; i < fallbackModules.Length && i < 9; i++)
        {
            KeyCode numberKey = KeyCode.Alpha1 + i;
            if (Input.GetKeyDown(numberKey))
            {
                SelectModuleByIndex(i);
                break;
            }
        }
    }

    private void SelectModuleByIndex(int index)
    {
        if (index < 0 || index >= fallbackModules.Length)
            return;

        selectedModuleIndex = index;
        selectedModule = fallbackModules[index];

        moveModeActive = false;
        hasMoveSource = false;

        string moduleName = string.IsNullOrWhiteSpace(selectedModule.moduleName)
            ? selectedModule.name
            : selectedModule.moduleName;

        Debug.Log($"Selected module [{selectedModuleIndex + 1}/{fallbackModules.Length}]: {moduleName}");
    }

    private void HandleModuleSelected(ModuleDefinition moduleDefinition)
    {
        selectedModule = moduleDefinition;
        moveModeActive = false;
        hasMoveSource = false;

        if (!selectedModule || fallbackModules.Length == 0)
            return;

        for (int i = 0; i < fallbackModules.Length; i++)
        {
            if (fallbackModules[i] == selectedModule)
            {
                selectedModuleIndex = i;
                break;
            }
        }
    }

    private void HandlePrimaryClick()
    {
        if (!TryGetGridCellUnderCursor(out var gridCell))
            return;

        if (moveModeActive)
        {
            HandleMoveClick(gridCell);
            return;
        }

        if (selectedModule)
            shipModuleBuilder.TryPlaceModule(selectedModule, gridCell);
    }

    private void HandleMoveClick(Vector2Int clickedCell)
    {
        if (!hasMoveSource)
        {
            if (shipModuleBuilder.GetModuleAt(clickedCell) == null)
                return;

            moveSourceCell = clickedCell;
            hasMoveSource = true;
            Debug.Log($"Move source selected: {moveSourceCell}");
            return;
        }

        bool moved = shipModuleBuilder.TryMoveModule(moveSourceCell, clickedCell);
        Debug.Log(moved
            ? $"Moved module: {moveSourceCell} -> {clickedCell}"
            : $"Move failed: {moveSourceCell} -> {clickedCell}");

        moveModeActive = false;
        hasMoveSource = false;
    }

    private void TryRemoveAtCursor()
    {
        if (!TryGetGridCellUnderCursor(out var gridCell))
            return;

        shipModuleBuilder.TryRemoveModule(gridCell);
    }

    private bool TryGetGridCellUnderCursor(out Vector2Int gridCell)
    {
        gridCell = default;

        if (!buildCamera)
        {
            buildCamera = Camera.main;
            return false;
        }

        Ray mouseRay = buildCamera.ScreenPointToRay(Input.mousePosition);
        Plane shipPlane = new Plane(
            shipModuleBuilder.GetBuildPlaneNormalWorld(),
            shipModuleBuilder.GetBuildPlanePointWorld());

        if (!shipPlane.Raycast(mouseRay, out float distance))
            return false;

        Vector3 hitPoint = mouseRay.GetPoint(distance);
        return shipModuleBuilder.TryWorldToGridPosition(hitPoint, out gridCell);
    }
}
