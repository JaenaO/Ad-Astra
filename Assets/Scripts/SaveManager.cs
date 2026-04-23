using UnityEngine;

public static class SaveManager
{
    private const string HasSaveKey = "HasSave";
    private const string AsteroidDustKey = "AsteroidDust";
    private const string SpaceCrystalKey = "SpaceCrystal";
    private const string StarFragmentKey = "StarFragment";
    private const string NovaflareKey = "Novaflare";

    public static void NewGame()
    {
        // Start fresh values, but DO NOT mark it as a saved game yet
        PlayerPrefs.SetInt(AsteroidDustKey, 0);
        PlayerPrefs.SetInt(SpaceCrystalKey, 0);
        PlayerPrefs.SetInt(StarFragmentKey, 0);
        PlayerPrefs.SetInt(NovaflareKey, 0);

        // Make sure "Load Previous Game" does not work until player actually saves
        PlayerPrefs.DeleteKey(HasSaveKey);

        PlayerPrefs.Save();
        Debug.Log("Started new game.");
    }

    public static bool HasSave()
    {
        return PlayerPrefs.GetInt(HasSaveKey, 0) == 1;
    }

    public static void SaveGame(int asteroidDust, int spaceCrystal, int starFragment, int novaflare)
    {
        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.SetInt(AsteroidDustKey, asteroidDust);
        PlayerPrefs.SetInt(SpaceCrystalKey, spaceCrystal);
        PlayerPrefs.SetInt(StarFragmentKey, starFragment);
        PlayerPrefs.SetInt(NovaflareKey, novaflare);

        PlayerPrefs.Save();
        Debug.Log("Game saved.");
    }

    public static void LoadGame()
    {
        Debug.Log("Game loaded.");
    }

    public static int GetSavedAsteroidDust()
    {
        return PlayerPrefs.GetInt(AsteroidDustKey, 0);
    }

    public static int GetSavedSpaceCrystal()
    {
        return PlayerPrefs.GetInt(SpaceCrystalKey, 0);
    }

    public static int GetSavedStarFragment()
    {
        return PlayerPrefs.GetInt(StarFragmentKey, 0);
    }

    public static int GetSavedNovaflare()
    {
        return PlayerPrefs.GetInt(NovaflareKey, 0);
    }
}