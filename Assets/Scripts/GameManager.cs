using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private string backgroundSceneName = "BackgroundScene";
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";
    [SerializeField] private string collectionSceneName = "AsteroidCollectionScene";
    [SerializeField] private string buildingSceneName = "BuildingScene";

    private Dictionary<MaterialTier, int> stockpile = new()
    {
        { MaterialTier.AsteroidDust,  0 },
        { MaterialTier.SpaceCrystal,  0 },
        { MaterialTier.StarFragment,  0 },
        { MaterialTier.Novaflare,     0 },
    };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(EnsureBackgroundLoaded());
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private IEnumerator EnsureBackgroundLoaded()
    {
        if (!SceneManager.GetSceneByName(backgroundSceneName).isLoaded)
        {
            yield return SceneManager.LoadSceneAsync(backgroundSceneName, LoadSceneMode.Additive);
        }
    }

    public void AddMaterial(MaterialTier tier, int amount)
    {
        stockpile[tier] += amount;
        Debug.Log($"+{amount} {tier}");
    }

    public void AddCredits(int amount)
    {
        AddMaterial(MaterialTier.AsteroidDust, amount);
    }

    public int GetStock(MaterialTier tier)
    {
        return stockpile[tier];
    }

    public bool Spend(MaterialTier tier, int amount)
    {
        if (stockpile[tier] < amount) return false;
        stockpile[tier] -= amount;
        return true;
    }

    public void SaveCurrentGame()
    {
        SaveManager.SaveGame(
            GetStock(MaterialTier.AsteroidDust),
            GetStock(MaterialTier.SpaceCrystal),
            GetStock(MaterialTier.StarFragment),
            GetStock(MaterialTier.Novaflare)
        );
    }

    public void LoadSavedGameIntoManager()
    {
        stockpile[MaterialTier.AsteroidDust] = SaveManager.GetSavedAsteroidDust();
        stockpile[MaterialTier.SpaceCrystal] = SaveManager.GetSavedSpaceCrystal();
        stockpile[MaterialTier.StarFragment] = SaveManager.GetSavedStarFragment();
        stockpile[MaterialTier.Novaflare] = SaveManager.GetSavedNovaflare();

        Debug.Log("Loaded saved values into GameManager.");
    }

    public void ResetForNewGame()
    {
        stockpile[MaterialTier.AsteroidDust] = 0;
        stockpile[MaterialTier.SpaceCrystal] = 0;
        stockpile[MaterialTier.StarFragment] = 0;
        stockpile[MaterialTier.Novaflare] = 0;

        Debug.Log("Reset GameManager for new game.");
    }

    public void GoToMainMenu()
    {
        Debug.Log("GameManager: GoToMainMenu called");
        StartCoroutine(GoToMainMenuRoutine());
    }

    private IEnumerator GoToMainMenuRoutine()
    {
        Debug.Log("GameManager: GoToMainMenuRoutine started");

        yield return StartCoroutine(EnsureBackgroundLoaded());

        if (!SceneManager.GetSceneByName(mainMenuSceneName).isLoaded)
        {
            Debug.Log("GameManager: Loading MainMenuScene additively");
            yield return SceneManager.LoadSceneAsync(mainMenuSceneName, LoadSceneMode.Additive);
        }

        Scene loadedMenuScene = SceneManager.GetSceneByName(mainMenuSceneName);
        if (loadedMenuScene.IsValid() && loadedMenuScene.isLoaded)
        {
            SceneManager.SetActiveScene(loadedMenuScene);
            Debug.Log("GameManager: Set active scene to MainMenuScene");
        }

        if (SceneManager.GetSceneByName(collectionSceneName).isLoaded)
        {
            Debug.Log("GameManager: Unloading AsteroidCollectionScene");
            yield return SceneManager.UnloadSceneAsync(collectionSceneName);
        }

        if (SceneManager.GetSceneByName(buildingSceneName).isLoaded)
        {
            Debug.Log("GameManager: Unloading BuildingScene");
            yield return SceneManager.UnloadSceneAsync(buildingSceneName);
        }

        Debug.Log("GameManager: GoToMainMenuRoutine finished");
    }

    public void StartNewGameScene()
    {// add debugger
        Debug.Log("GameManager: StartNewGameScene called");
        StartCoroutine(StartNewGameSceneRoutine());
    }

    private IEnumerator StartNewGameSceneRoutine()
    {
        Debug.Log("GameManager: StartNewGameSceneRoutine started");

        yield return StartCoroutine(EnsureBackgroundLoaded());

        if (!SceneManager.GetSceneByName(collectionSceneName).isLoaded)
        {
            Debug.Log("GameManager: Loading AsteroidCollectionScene additively");
            yield return SceneManager.LoadSceneAsync(collectionSceneName, LoadSceneMode.Additive);
        }

        Scene loadedCollectionScene = SceneManager.GetSceneByName(collectionSceneName);
        if (loadedCollectionScene.IsValid() && loadedCollectionScene.isLoaded)
        {
            SceneManager.SetActiveScene(loadedCollectionScene);
            Debug.Log("GameManager: Set active scene to AsteroidCollectionScene");
        }

        if (SceneManager.GetSceneByName(buildingSceneName).isLoaded)
        {
            Debug.Log("GameManager: Unloading BuildingScene");
            yield return SceneManager.UnloadSceneAsync(buildingSceneName);
        }

        if (SceneManager.GetSceneByName(mainMenuSceneName).isLoaded)
        {
            Debug.Log("GameManager: Unloading MainMenuScene");
            yield return SceneManager.UnloadSceneAsync(mainMenuSceneName);
        }

        Debug.Log("GameManager: StartNewGameSceneRoutine finished");
    }

    public void LoadPreviousGameScene()
    {
        StartCoroutine(LoadPreviousGameSceneRoutine());
    }

    private IEnumerator LoadPreviousGameSceneRoutine()
    {
        Debug.Log("GameManager: LoadPreviousGameSceneRoutine started");

        yield return StartCoroutine(EnsureBackgroundLoaded());

        if (!SceneManager.GetSceneByName(collectionSceneName).isLoaded)
        {
            Debug.Log("GameManager: Loading AsteroidCollectionScene additively");
            yield return SceneManager.LoadSceneAsync(collectionSceneName, LoadSceneMode.Additive);
        }

        Scene loadedCollectionScene = SceneManager.GetSceneByName(collectionSceneName);
        if (loadedCollectionScene.IsValid() && loadedCollectionScene.isLoaded)
        {
            SceneManager.SetActiveScene(loadedCollectionScene);
            Debug.Log("GameManager: Set active scene to AsteroidCollectionScene");
        }

        if (SceneManager.GetSceneByName(mainMenuSceneName).isLoaded)
        {
            Debug.Log("GameManager: Unloading MainMenuScene");
            yield return SceneManager.UnloadSceneAsync(mainMenuSceneName);
        }

        if (SceneManager.GetSceneByName(buildingSceneName).isLoaded)
        {
            Debug.Log("GameManager: Unloading BuildingScene");
            yield return SceneManager.UnloadSceneAsync(buildingSceneName);
        }

        Debug.Log("GameManager: LoadPreviousGameSceneRoutine finished");
    }

    public void SwapGameplayScene()
    {
        StartCoroutine(SwapGameplaySceneRoutine());
    }

    private IEnumerator SwapGameplaySceneRoutine()
    {
        yield return StartCoroutine(EnsureBackgroundLoaded());

        if (SceneManager.GetSceneByName(collectionSceneName).isLoaded)
        {
            yield return SceneManager.LoadSceneAsync(buildingSceneName, LoadSceneMode.Additive);

            Scene loadedBuildingScene = SceneManager.GetSceneByName(buildingSceneName);
            if (loadedBuildingScene.IsValid() && loadedBuildingScene.isLoaded)
            {
                SceneManager.SetActiveScene(loadedBuildingScene);
                Debug.Log("GameManager: Set active scene to BuildingScene");
            }

            yield return SceneManager.UnloadSceneAsync(collectionSceneName);
        }
        else if (SceneManager.GetSceneByName(buildingSceneName).isLoaded)
        {
            yield return SceneManager.LoadSceneAsync(collectionSceneName, LoadSceneMode.Additive);

            Scene loadedCollectionScene = SceneManager.GetSceneByName(collectionSceneName);
            if (loadedCollectionScene.IsValid() && loadedCollectionScene.isLoaded)
            {
                SceneManager.SetActiveScene(loadedCollectionScene);
                Debug.Log("GameManager: Set active scene to AsteroidCollectionScene");
            }

            yield return SceneManager.UnloadSceneAsync(buildingSceneName);
        }
    }
}