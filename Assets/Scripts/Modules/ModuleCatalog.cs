using System;
using System.Collections.Generic;
using UnityEngine;

public static class ModuleCatalog
{
    private static readonly ModuleCategory[] DefaultBuildableCategories =
    {
        ModuleCategory.Generator,
        ModuleCategory.Converter,
        ModuleCategory.Engine,
        ModuleCategory.Claw,
    };

    public static ModuleDefinition[] LoadModules(string resourcePath = "ModuleData")
    {
        string path = string.IsNullOrWhiteSpace(resourcePath) ? "ModuleData" : resourcePath;
        return Resources.LoadAll<ModuleDefinition>(path);
    }

    public static ModuleDefinition[] LoadBuildableModules(string resourcePath = "ModuleData")
    {
        return LoadBuildableModules(resourcePath, DefaultBuildableCategories);
    }

    public static ModuleDefinition[] LoadBuildableModules(string resourcePath, ModuleCategory[] allowedCategories)
    {
        var buildableModules = new List<ModuleDefinition>();

        foreach (var module in LoadModules(resourcePath))
        {
            if (!module || !module.prefab)
                continue;

            if (string.Equals(module.id, "BASIC_CORE", StringComparison.OrdinalIgnoreCase))
                continue;

            if (string.Equals(module.id, "BASIC_BLOCK", StringComparison.OrdinalIgnoreCase) || IsAllowedCategory(module.category, allowedCategories))
                buildableModules.Add(module);
        }

        return buildableModules.ToArray();
    }

    private static bool IsAllowedCategory(ModuleCategory category, ModuleCategory[] allowedCategories)
    {
        if (allowedCategories == null || allowedCategories.Length == 0)
            return false;

        return Array.IndexOf(allowedCategories, category) >= 0;
    }
}