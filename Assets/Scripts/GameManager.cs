using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private TMP_Text dustText;
    private TMP_Text crystalText;
    private TMP_Text fragmentText;
    private TMP_Text novaflareText;

    private Dictionary<MaterialTier, int> stockpile = new()
    {
        { MaterialTier.AsteroidDust,  0 },
        { MaterialTier.SpaceCrystal,  0 },
        { MaterialTier.StarFragment,  0 },
        { MaterialTier.Novaflare,     0 },
    };

    public int credits = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Find text objects by name automatically - no dragging needed
        dustText = GameObject.Find("Dust")?.GetComponent<TMP_Text>();
        crystalText = GameObject.Find("Crystal")?.GetComponent<TMP_Text>();
        fragmentText = GameObject.Find("Fragment")?.GetComponent<TMP_Text>();
        novaflareText = GameObject.Find("Novaflare")?.GetComponent<TMP_Text>();

            if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject); // survives scene changes
    }
    else Destroy(gameObject);
    }

    public void AddMaterial(MaterialTier tier, int amount)
    {
        stockpile[tier] += amount;
        Debug.Log($"+{amount} {tier}");
        UpdateUI();
    }

    public void AddCredits(int amount)
    {
        credits += amount;
        UpdateUI();
    }

    public int GetStock(MaterialTier tier) => stockpile[tier];

    public bool Spend(MaterialTier tier, int amount)
    {
        if (stockpile[tier] < amount) return false;
        stockpile[tier] -= amount;
        UpdateUI();
        return true;
    }

    void UpdateUI()
    {
        if (dustText) dustText.text = $"Dust: {stockpile[MaterialTier.AsteroidDust]}";
        if (crystalText) crystalText.text = $"Crystal: {stockpile[MaterialTier.SpaceCrystal]}";
        if (fragmentText) fragmentText.text = $"Fragment: {stockpile[MaterialTier.StarFragment]}";
        if (novaflareText) novaflareText.text = $"Novaflare: {stockpile[MaterialTier.Novaflare]}";
    }

}