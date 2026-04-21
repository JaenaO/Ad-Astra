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
            
        private GameObject[,] ModuleGrid = new GameObject[HorizontalGridTiles, VerticalGridTiles];
        private Dictionary<string, ModuleDefinition> modulesById;

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
         private GameObject PlaceModule(ModuleDefinition moduleDefinition, Vector2Int gridPosition) {
            var newModule = Instantiate(moduleDefinition.prefab, GridToLocalPosition(gridPosition), Quaternion.identity, transform);
            ModuleGrid[CenterIndex.x + gridPosition.x, CenterIndex.y + gridPosition.y] = newModule;
            
            return newModule;
        }
        
        void Awake()
        {
            //Load all the module data
            modulesById = new Dictionary<string, ModuleDefinition>();
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

                if (modulesById.ContainsKey(module.id))
                {
                    Debug.LogWarning($"Duplicate module id found: '{module.id}'.");
                    continue;
                }

                modulesById.Add(module.id, module);
            }

            Debug.Log($"Loaded {modulesById.Count} module definitions.");
        }
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            //PLacing starter ship
            PlaceModule(modulesById["BASIC_CORE"], Vector2Int.zero);
            PlaceModule(modulesById["BASIC_ENGINE"], Vector2Int.down);
            PlaceModule(modulesById["BASIC_BLOCK"], Vector2Int.up);
            PlaceModule(modulesById["BASIC_BLOCK"], Vector2Int.left);
            PlaceModule(modulesById["BASIC_BLOCK"], Vector2Int.right);
        }
    }
}
