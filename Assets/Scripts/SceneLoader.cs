// Assets/Scripts/SceneLoader.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadAsteroids() => SceneManager.LoadScene("AsteroidCollectionScene");
    public void LoadShipModule() => SceneManager.LoadScene("ModuleScene");
}