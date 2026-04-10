using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour {
  public GameObject asteroidPrefab;
  public float maxSpawnTime = 5.0f; // Time in seconds between each spawn is randomized between 0 and this value
  private Vector3 screenBounds;

  void Start() {
    float depth = Mathf.Abs(Camera.main.transform.position.z);
    screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, depth));
    StartCoroutine(SpawnAsteroids());
  }

  private void SpawnObject() {
    GameObject asteroid = Instantiate(asteroidPrefab) as GameObject;
    asteroid.transform.position = new Vector3(Random.Range(screenBounds.x * -1, screenBounds.x), screenBounds.y, 0);
  }

  IEnumerator SpawnAsteroids() {
    while (true) {
      yield return new WaitForSeconds(Random.Range(0, maxSpawnTime));
      SpawnObject();
    }
  }
}