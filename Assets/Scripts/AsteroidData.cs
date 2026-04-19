using UnityEngine;

public enum MaterialTier { AsteroidDust, SpaceCrystal, StarFragment, Novaflare }

[System.Serializable]
public class LootEntry
{
    public MaterialTier tier;
    public float weight;      // 60 / 25 / 13 / 2
    public int minAmount;
    public int maxAmount;
}

[CreateAssetMenu(fileName = "AsteroidData", menuName = "AdAstra/AsteroidData")]
public class AsteroidData : ScriptableObject
{
    public string asteroidName;
    public Color color;
    public Vector3 scale = Vector3.one;
    public float spawnWeight;

    [Header("Loot")]
    public LootEntry[] lootTable;

    public void DropLoot(Vector3 position)
    {
        if (lootTable == null || lootTable.Length == 0)
        {
            GameManager.Instance.AddCredits(10); // fallback
            return;
        }

        float total = 0f;
        foreach (var e in lootTable) total += e.weight;

        float roll = Random.Range(0f, total);
        float cumulative = 0f;
        foreach (var e in lootTable)
        {
            cumulative += e.weight;
            if (roll <= cumulative)
            {
                int amount = Random.Range(e.minAmount, e.maxAmount + 1);
                GameManager.Instance.AddMaterial(e.tier, amount);
                return;
            }
        }
    }
}