using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
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
        buttonList.Add(placeRoadButton);

        placeRoadButton.onClick.AddListener(() =>
        {
            ResetButtonColors();
            ModifyOutline(placeRoadButton);
            OnRoadPlacement?.Invoke();
        });

        BuildModuleButtons();
    }

    void BuildModuleButtons()
    {
        // Load all modules from Resources automatically
        availableModules = Resources.LoadAll<ModuleDefinition>("ModuleData");

        foreach (var module in availableModules)
        {
            GameObject btnObj = Instantiate(moduleButtonPrefab, moduleButtonContainer);
            var btn = btnObj.GetComponent<Button>();
            var label = btnObj.GetComponentInChildren<TMP_Text>();

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