using UnityEngine;


[CreateAssetMenu(fileName = "NewModule", menuName = "AdAstra/Upgrade")]
public class UpgradeDefinition : ScriptableObject
{
    [Header("Identity")]
    public string upgradeName;

    public UpgradeDefinition directChild;       // Basically defines what the next module in this line would be
    public UpgradeDefinition[] branchChildren;  // D

    [Header("Upgrade")]
    public ModuleDefinition[] unlocks;      // What modules this tech unlocks
}