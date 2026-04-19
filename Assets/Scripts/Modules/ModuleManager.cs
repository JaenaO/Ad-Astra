using UnityEngine;
using System.Collections.Generic;

public class ModuleManager : MonoBehaviour
{
    public static ModuleManager Instance;

    private HashSet<ModuleDefinition> unlockedModules = new();
    private int currentWeight = 0;
    private int maxWeight = 4; // base: 2 engines at -1 weight +3 bonus each = 4

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool CanAfford(ModuleDefinition mod)
    {
        foreach (var cost in mod.costs)
            if (GameManager.Instance.GetStock(cost.tier) < cost.amount) return false;
        return true;
    }

    public bool PrerequisitesMet(ModuleDefinition mod)
    {
        foreach (var prereq in mod.prerequisites)
            if (!unlockedModules.Contains(prereq)) return false;
        return true;
    }

    public bool HasWeightCapacity(ModuleDefinition mod)
    {
        return (currentWeight + mod.weightCost) <= maxWeight;
    }

    public bool TryPurchase(ModuleDefinition mod)
    {
        if (!CanAfford(mod) || !PrerequisitesMet(mod) || !HasWeightCapacity(mod))
            return false;

        // Deduct costs
        foreach (var cost in mod.costs)
            GameManager.Instance.Spend(cost.tier, cost.amount);

        currentWeight += mod.weightCost;
        maxWeight += mod.maxWeightBonus;
        unlockedModules.Add(mod);

        // Spawn and activate the module
        GameObject obj = new GameObject($"Module_{mod.moduleName}");
        obj.transform.SetParent(transform);
        var instance = obj.AddComponent<ModuleInstance>();
        instance.definition = mod;
        instance.Activate();

        Debug.Log($"Built: {mod.moduleName} | Weight: {currentWeight}/{maxWeight}");
        return true;
    }

    public bool IsUnlocked(ModuleDefinition mod) => unlockedModules.Contains(mod);
    public int GetCurrentWeight() => currentWeight;
    public int GetMaxWeight() => maxWeight;
}