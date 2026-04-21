using UnityEngine;

namespace Modules
{
    public enum ModuleType
    {
        Core,
        Decor,
        Generator,
        Engine,
        Collector,
        Converter,
    }
    
    [CreateAssetMenu(fileName = "ModuleDefinition", menuName = "AdAstra/ModuleData")]
    public class ModuleDefinition  : ScriptableObject
    {
        public string id;               // Internal ID used by the game to reference
        public string displayName;      // Name that interfaces use to display to players
        public ModuleType moduleType;   // Type of module that this is
        public GameObject prefab;       // The associated prefab to this stat block
        
        [Header("Building stats")]
        public int cost;
        public float weight;
        
        //[Header("Unlock stats")]
        //Not made yet
        

    }
}

