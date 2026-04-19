using UnityEngine;
using System.Collections.Generic;

public class ModuleManager : MonoBehaviour
{
    public static ModuleManager Instance;

    private HashSet<ModuleDefinition> unlockedModules = new();
    private int currentWeight = 0;
    private int maxWeight = 4;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
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
        if (mod.prerequisites == null) return true;
        foreach (var prereq in mod.prerequisites)
            if (!unlockedModules.Contains(prereq)) return false;
        return true;
    }

    public bool HasWeightCapacity(ModuleDefinition mod)
    {
        return (currentWeight + mod.weightCost) <= maxWeight;
    }

    // Used by the shop UI (ModuleScene)
    public bool TryPurchase(ModuleDefinition mod)
    {
        if (!CanAfford(mod) || !PrerequisitesMet(mod) || !HasWeightCapacity(mod))
            return false;

        foreach (var cost in mod.costs)
            GameManager.Instance.Spend(cost.tier, cost.amount);

        ActivateModule(mod);
        return true;
    }

    // Used by the grid placement system (StructureManager)
    public void ActivateModule(ModuleDefinition mod)
    {
        unlockedModules.Add(mod);
        currentWeight += mod.weightCost;
        maxWeight += mod.maxWeightBonus;

        GameObject obj = new GameObject($"Module_{mod.moduleName}");
        obj.transform.SetParent(transform);
        var instance = obj.AddComponent<ModuleInstance>();
        instance.definition = mod;
        instance.Activate();

        Debug.Log($"Built: {mod.moduleName} | Weight: {currentWeight}/{maxWeight}");
    }

    public bool IsUnlocked(ModuleDefinition mod) => unlockedModules.Contains(mod);
    public int GetCurrentWeight() => currentWeight;
    public int GetMaxWeight() => maxWeight;
}