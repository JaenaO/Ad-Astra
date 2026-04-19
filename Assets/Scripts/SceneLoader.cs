using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    void Start()
    {
        GameObject btnObj = GameObject.Find("SceneSwapButton");
        if (btnObj == null)
        {
            Debug.LogError("SceneSwapButton not found!");
            return;
        }

        Button btn = btnObj.GetComponent<Button>();
        if (btn == null)
        {
            Debug.LogError("No Button component on SceneSwapButton!");
            return;
        }

        btn.onClick.AddListener(LoadShipScene);
        Debug.Log("Button wired successfully!");
    }

    public void LoadShipScene() => SceneManager.LoadScene("ModuleScene");
    public void LoadAsteroidScene() => SceneManager.LoadScene("AsteroidCollectionScene");
}