using UnityEngine;

public enum ModuleCategory { Engine, Generator, Converter, Claw }

[System.Serializable]
public class ResourceCost
{
    public MaterialTier tier;
    public int amount;
}

[CreateAssetMenu(fileName = "NewModule", menuName = "AdAstra/Module")]
public class ModuleDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public GameObject prefab;
    public string moduleName;
    public ModuleCategory category;
    public ModuleDefinition[] prerequisites; // parent nodes in the tree

    [Header("Cost")]
    public ResourceCost[] costs;

    [Header("Weight")]
    public int weightCost;      // negative impact on ship weight capacity

    [Header("Engine Stats")]
    public int maxWeightBonus;

    [Header("Generator Stats")]
    public int passiveAD;       // Asteroid Dust per tick
    public int passiveSC;       // Space Crystal per tick
    public int passiveF;        // Star Fragment per tick
    public int passiveN;        // Novaflare per tick
    public float tickInterval;  // seconds between passive generation

    [Header("Converter Stats")]
    public MaterialTier convertFrom;
    public int convertFromAmount;
    public MaterialTier convertTo;
    public int convertToAmount;
    public float convertInterval; // seconds per conversion
    public float convertSpeedMultiplier = 1f; // Speed I/II/III

    [Header("Claw Stats")]
    public int clawsUnlocked;           // how many claw arms this adds
    public float cooldownReduction;     // Fast Grappling I/II
    public float luckBonus;             // Lucky I/II (probability shift)
    public int yieldRangeBonus;         // More More I/II/III
}