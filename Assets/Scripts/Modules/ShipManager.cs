using System;
using System.Collections.Generic;
using Mono.Cecil;
using Unity.Mathematics;
using UnityEngine;

namespace Modules
{
    public class ShipModuleBuilder : MonoBehaviour
    {
        private const int GridScale = 2;
        private const int VerticalGridTiles = 10;
        private const int HorizontalGridTiles = 5;

        private readonly Vector2Int CenterIndex = new (HorizontalGridTiles/2, VerticalGridTiles/2);
        //Change this center index to be closer to the bottom
        
        private GameObject[,] ModuleGrid = new GameObject[HorizontalGridTiles, VerticalGridTiles];
        private Dictionary<string, ModuleDefinition> ModuleStorage;

        [NonSerialized]
        public int CurrentWeight = 0;
        [NonSerialized]
        public int MaxWeight = 0;
        [NonSerialized]
        public int CurrentEngines = 0;
        [NonSerialized]
        public int MaxEngines = 2;
        
        
        /// <summary>
        /// Turns a grid position into local position based off grid size
        /// </summary>
        /// <param name="gridPosition">Current position in the building grid</param>
        /// <returns>Local position based off grid size</returns>
        private Vector3 GridToLocalPosition(Vector2Int gridPosition)
        {
            return new Vector3(gridPosition.x * GridScale, gridPosition.y * GridScale, 0);
        }
        
        /// <summary>
        /// Places a module at given grid position relative to center
        /// </summary>
        /// <param name="moduleDefinition"></param>
        /// <param name="gridPosition"></param>
        /// <returns>Newly created module</returns>
         public GameObject PlaceModule(ModuleDefinition moduleDefinition, Vector2Int gridPosition) {
            var newModule = Instantiate(moduleDefinition.prefab, GridToLocalPosition(gridPosition), Quaternion.identity, transform);
            ModuleGrid[CenterIndex.x + gridPosition.x, CenterIndex.y + gridPosition.y] = newModule;
            
            //Apply weight change
            MaxWeight += moduleDefinition.maxWeightBonus;
            if (moduleDefinition.category == ModuleCategory.Engine)
                CurrentEngines += 1;

            var instance = newModule.AddComponent<ModuleInstance>();
            instance.definition = moduleDefinition;
            instance.Activate();
            
            return newModule;
        }

        /// <summary>
        /// If placement would be valid
        /// </summary>
        /// <param name="moduleDefinition"></param>
        /// <param name="gridPosition"></param>
        /// <returns>If we can currently place part in selected position</returns>
        public bool CanPlaceModule(ModuleDefinition moduleDefinition, Vector2Int gridPosition)
        {
            // Checking if outside grid boundaries
            if (gridPosition.x > HorizontalGridTiles/2 || gridPosition.y > VerticalGridTiles/2)
                return false;

            // Weight check
            if (CurrentWeight + moduleDefinition.weightCost > MaxWeight)
                return false;
            
            //Engine limit check
            if (moduleDefinition.category == ModuleCategory.Engine && CurrentEngines+1 > MaxEngines)
                return false;
            
            // Cost check
            foreach (var cost in moduleDefinition.costs)
                if (GameManager.Instance.GetStock(cost.tier) < cost.amount) return false;
            
            return true;
        }
        
        void Awake()
        {
            //Load all the module data
            ModuleStorage = new Dictionary<string, ModuleDefinition>();
            foreach (ModuleDefinition module in Resources.LoadAll<ModuleDefinition>("ModuleData"))
            {
                if (module == null)
                    continue;

                if (string.IsNullOrWhiteSpace(module.id))
                {
                    Debug.LogWarning($"Module asset '{module.name}' has no id.");
                    continue;
                }

                if (module.prefab == null)
                {
                    Debug.LogWarning($"Module '{module.id}' has no prefab assigned.");
                    continue;
                }

                if (ModuleStorage.ContainsKey(module.id))
                {
                    Debug.LogWarning($"Duplicate module id found: '{module.id}'.");
                    continue;
                }

                ModuleStorage.Add(module.id, module);
            }

            Debug.Log($"Loaded {ModuleStorage.Count} module definitions.");
        }
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            //PLacing starter ship
            PlaceModule(ModuleStorage["BASIC_CORE"], Vector2Int.zero);
            PlaceModule(ModuleStorage["BASIC_ENGINE"], Vector2Int.down);
            PlaceModule(ModuleStorage["BASIC_BLOCK"], Vector2Int.up);
            PlaceModule(ModuleStorage["BASIC_BLOCK"], Vector2Int.left);
            PlaceModule(ModuleStorage["BASIC_BLOCK"], Vector2Int.right);
        }
    }
}
