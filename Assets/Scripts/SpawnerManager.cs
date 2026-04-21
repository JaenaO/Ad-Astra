using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnerManager : MonoBehaviour
{
    public GameObject asteroidPrefab;
    private AsteroidData[] asteroidTypes; // drag all 4 assets here
    public float maxSpawnTime = 5.0f;
    private Vector3 screenBounds;

    private void Awake()
    {
        asteroidTypes = Resources.LoadAll<AsteroidData>("AsteroidData"); 
    }

    void Start()
    {
        float depth = Mathf.Abs(Camera.main.transform.position.z);
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, depth));
        StartCoroutine(SpawnAsteroids());
    }

    private void SpawnObject()
    {
        GameObject asteroidObj = Instantiate(asteroidPrefab);
        asteroidObj.transform.position = new Vector3(
            Random.Range(screenBounds.x * -1, screenBounds.x), screenBounds.y, 0);

        asteroid a = asteroidObj.GetComponent<asteroid>();
        if (a != null && asteroidTypes.Length > 0)
            a.data = PickRarity();
    }

    AsteroidData PickRarity()
    {
        float totalWeight = 0f;
        foreach (var t in asteroidTypes) totalWeight += t.spawnWeight;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        foreach (var t in asteroidTypes)
        {
            cumulative += t.spawnWeight;
            if (roll <= cumulative) return t;
        }
        return asteroidTypes[0];
    }

    IEnumerator SpawnAsteroids()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(0, maxSpawnTime));
            SpawnObject();
        }
    }
}