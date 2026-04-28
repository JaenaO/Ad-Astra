using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    private const string ModuleResourcePath = "ModuleData";

    // Keep existing road placement
    public Action OnRoadPlacement;
    public Button placeRoadButton;

    // Module shop panel
    public GameObject moduleButtonPrefab;
    public Transform moduleButtonContainer;
    public ModuleDefinition[] availableModules;

    public Color outlineColor;

    // Fired when player selects a module to place
    public Action<ModuleDefinition> OnModuleSelected;

    private List<Button> buttonList = new();

    private void Start()
    {
        if (placeRoadButton)
        {
            buttonList.Add(placeRoadButton);

            placeRoadButton.onClick.AddListener(() =>
            {
                ResetButtonColors();
                ModifyOutline(placeRoadButton);
                OnRoadPlacement?.Invoke();
            });
        }
        else
        {
            Debug.LogWarning("UIController: Place Road Button is not assigned.");
        }

        BuildModuleButtons();
    }

    void BuildModuleButtons()
    {
        if (!moduleButtonPrefab || !moduleButtonContainer)
        {
            Debug.LogWarning("UIController: Module button prefab/container is not assigned.");
            return;
        }

        // Load buildable modules from Resources automatically.
        availableModules = ModuleCatalog.LoadBuildableModules(ModuleResourcePath);

        foreach (var module in availableModules)
        {
            GameObject btnObj = Instantiate(moduleButtonPrefab, moduleButtonContainer);
            var btn = btnObj.GetComponent<Button>();
            var label = btnObj.GetComponentInChildren<TMP_Text>();

            if (!btn || !label)
            {
                Debug.LogWarning("UIController: Module button prefab must contain Button and TMP_Text.");
                continue;
            }

            if (!label.font && TMP_Settings.defaultFontAsset)
                label.font = TMP_Settings.defaultFontAsset;

            label.text = FormatLabel(module);

            var captured = module;
            btn.onClick.AddListener(() =>
            {
                ResetButtonColors();
                ModifyOutline(btn);
                OnModuleSelected?.Invoke(captured);
            });

            buttonList.Add(btn);
        }
        if (availableModules.Length == 0)
            Debug.LogWarning("UIController: No buildable modules found in Resources/ModuleData.");
    }

    string FormatLabel(ModuleDefinition mod)
    {
        if (!mod)
            return "Unknown Module";

        string moduleName = string.IsNullOrWhiteSpace(mod.moduleName) ? mod.name : mod.moduleName;
        ResourceCost[] costs = mod.costs ?? Array.Empty<ResourceCost>();

        if (costs.Length == 0)
            return moduleName;

        var costLines = new List<string>();
        foreach (var cost in costs)
        {
            if (cost == null)
                continue;

            costLines.Add($"{cost.amount} {cost.tier}");
        }

        if (costLines.Count == 0)
            return moduleName;

        return $"{moduleName}\n<size=70%>{string.Join("\n", costLines)}</size>";
    }

    private void ModifyOutline(Button button)
    {
        var outline = button.GetComponent<Outline>();
        if (outline)
        {
            outline.effectColor = outlineColor;
            outline.enabled = true;
        }
    }

    private void ResetButtonColors()
    {
        foreach (var button in buttonList)
        {
            var outline = button.GetComponent<Outline>();
            if (outline) outline.enabled = false;
        }
    }
}