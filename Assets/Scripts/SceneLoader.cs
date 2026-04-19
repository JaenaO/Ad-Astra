using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    void Start()
    {
        // Find the button by name and wire it up automatically
        GameObject btnObj = GameObject.Find("SceneSwapButton");
        if (btnObj != null)
        {
            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(LoadShipScene);
        }
    }

    public void LoadShipScene() => SceneManager.LoadScene("ModuleScene");
    public void LoadAsteroidScene() => SceneManager.LoadScene("AsteroidCollectionScene");
}