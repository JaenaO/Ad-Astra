// Assets/Scripts/AsteroidData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "AsteroidData", menuName = "AdAstra/AsteroidData")]
public class AsteroidData : ScriptableObject
{
    public string asteroidName;
    public int creditValue;
    public Color color;
    public Vector3 scale = Vector3.one;
    public float spawnWeight; // higher = more common
}