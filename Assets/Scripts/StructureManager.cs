using UnityEngine;

public class StructureManager : MonoBehaviour
{
    public void PlaceModule(Vector3Int position, ModuleDefinition module)
    {
        if (!ModuleManager.Instance.CanAfford(module))
        {
            Debug.Log($"Can't afford {module.moduleName}");
            return;
        }

        if (!ModuleManager.Instance.PrerequisitesMet(module))
        {
            Debug.Log($"Prerequisites not met for {module.moduleName}");
            return;
        }

        if (!ModuleManager.Instance.HasWeightCapacity(module))
        {
            Debug.Log($"Not enough weight capacity");
            return;
        }

        foreach (var cost in module.costs)
            GameManager.Instance.Spend(cost.tier, cost.amount);

        ModuleManager.Instance.ActivateModule(module);
        Debug.Log($"Placed {module.moduleName}!");
    }
}