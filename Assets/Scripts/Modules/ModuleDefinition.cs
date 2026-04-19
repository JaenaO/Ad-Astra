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
    public string id;           // e.g. "BASIC_ENGINE" — must match ShipManager strings
    public string moduleName;
    public GameObject prefab;   // the 3D prefab to place on the grid
    public ModuleCategory category;
    public ModuleDefinition[] prerequisites;

    [Header("Cost")]
    public ResourceCost[] costs;

    [Header("Weight")]
    public int weightCost;
    public int maxWeightBonus;

    [Header("Generator Stats")]
    public int passiveAD;
    public int passiveSC;
    public int passiveF;
    public int passiveN;
    public float tickInterval;

    [Header("Converter Stats")]
    public MaterialTier convertFrom;
    public int convertFromAmount;
    public MaterialTier convertTo;
    public int convertToAmount;
    public float convertInterval;
    public float convertSpeedMultiplier = 1f;

    [Header("Claw Stats")]
    public int clawsUnlocked;
    public float cooldownReduction;
    public float luckBonus;
    public int yieldRangeBonus;
}