using System;
using UnityEngine;

public class ModuleShop : MonoBehaviour
{
    [SerializeField] private GameObject buttonTemplate;
    [SerializeField] private Transform contentParent;
    
    public event Action<ModuleDefinition> OnModuleSelected;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ModuleDefinition[] modules = ModuleCatalog.LoadBuildableModules("ModuleData");

        foreach (ModuleDefinition module in modules)
        {
            if (string.IsNullOrWhiteSpace(module.moduleName))
                continue;

            GameObject buttonObj = Instantiate(buttonTemplate, contentParent);
            buttonObj.SetActive(true); // important if template is disabled

            // Set text
            //var text = buttonObj.GetComponentInChildren<TMPro.TMP_Text>();
            var titleBox = buttonObj.transform.Find("ModuleTitle").GetComponent<TMPro.TMP_Text>();
            titleBox.text = module.moduleName;
            
            //Setting description
            if (!string.IsNullOrWhiteSpace(module.description))
                buttonObj.transform.Find("ModuleDescription").GetComponent<TMPro.TMP_Text>().text = module.description;  
            
            //Setting icon
            if (module.icon != null)
                buttonObj.transform.Find("Image").GetComponent<UnityEngine.UI.Image>().sprite = module.icon;

            // Button click
            var button = buttonObj.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() => SelectModule(module));
        }
    }

    private void SelectModule(ModuleDefinition module)
    {
        OnModuleSelected?.Invoke(module);
    }
}
