using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private string backgroundSceneName = "BackgroundScene";
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

            if (!SceneManager.GetSceneByName(backgroundSceneName).isLoaded)
            {
                SceneManager.LoadScene(backgroundSceneName, LoadSceneMode.Additive);
            }
        }
        else
        {
            Destroy(gameObject);
            return;
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

    public void SwapGameplayScene()
    {
        StartCoroutine(SwapGameplaySceneRoutine());
    }

    private IEnumerator SwapGameplaySceneRoutine()
    {
        if (SceneManager.GetSceneByName(collectionSceneName).isLoaded)
        {
            yield return SceneManager.UnloadSceneAsync(collectionSceneName);
            yield return SceneManager.LoadSceneAsync(buildingSceneName, LoadSceneMode.Additive);
        }
        else if (SceneManager.GetSceneByName(buildingSceneName).isLoaded)
        {
            yield return SceneManager.UnloadSceneAsync(buildingSceneName);
            yield return SceneManager.LoadSceneAsync(collectionSceneName, LoadSceneMode.Additive);
        }
    }
}