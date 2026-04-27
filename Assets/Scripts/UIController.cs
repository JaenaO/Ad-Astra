using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    private static readonly ModuleCategory[] BuildableCategories =
    {
        ModuleCategory.Generator,
        ModuleCategory.Converter,
        ModuleCategory.Engine,
        ModuleCategory.Claw,
    };

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
        if (placeRoadButton != null)
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
        if (moduleButtonPrefab == null || moduleButtonContainer == null)
        {
            Debug.LogWarning("UIController: Module button prefab/container is not assigned.");
            return;
        }

        // Load buildable modules from Resources automatically.
        availableModules = LoadBuildableModules();

        foreach (var module in availableModules)
        {
            GameObject btnObj = Instantiate(moduleButtonPrefab, moduleButtonContainer);
            var btn = btnObj.GetComponent<Button>();
            var label = btnObj.GetComponentInChildren<TMP_Text>();

            if (btn == null || label == null)
            {
                Debug.LogWarning("UIController: Module button prefab must contain Button and TMP_Text.");
                continue;
            }

            if (label.font == null && TMP_Settings.defaultFontAsset != null)
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
    }

    private ModuleDefinition[] LoadBuildableModules()
    {
        var loadedModules = Resources.LoadAll<ModuleDefinition>("ModuleData");
        var buildableModules = new List<ModuleDefinition>();

        foreach (var module in loadedModules)
        {
            if (module == null || module.prefab == null)
                continue;

            if (string.Equals(module.id, "BASIC_CORE", StringComparison.OrdinalIgnoreCase))
                continue;

            if (string.Equals(module.id, "BASIC_BLOCK", StringComparison.OrdinalIgnoreCase))
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

        if (buildableModules.Count == 0)
            Debug.LogWarning("UIController: No buildable modules found in Resources/ModuleData.");

        return buildableModules.ToArray();
    }

    string FormatLabel(ModuleDefinition mod)
    {
        string cost = "";
        foreach (var c in mod.costs)
            cost += $"{c.amount} {c.tier}\n";
        return $"{mod.moduleName}\n<size=70%>{cost.Trim()}</size>";
    }

    private void ModifyOutline(Button button)
    {
        var outline = button.GetComponent<Outline>();
        if (outline != null)
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
            if (outline != null) outline.enabled = false;
        }
    }
}