using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private float speedMultiplier = 0.2f;
    [SerializeField] private float asteroidSpeed = 10f;

    private Transform[] quads;
    private float spacing;

    void Start()
    {
        int count = transform.childCount;
        quads = new Transform[count];

        for (int i = 0; i < count; i++)
        {
            quads[i] = transform.GetChild(i);
        }

        if (count >= 2)
        {
            spacing = Mathf.Abs(quads[1].position.y - quads[0].position.y);
        }
        else
        {
            spacing = 100f;
        }
    }

    void Update()
    {
        float moveSpeed = asteroidSpeed * speedMultiplier;

        foreach (Transform quad in quads)
        {
            quad.position += Vector3.down * moveSpeed * Time.deltaTime;
        }

        for (int i = 0; i < quads.Length; i++)
        {
            if (quads[i].position.y <= -spacing)
            {
                float highestY = GetHighestY();
                quads[i].position = new Vector3(
                    quads[i].position.x,
                    highestY + spacing,
                    quads[i].position.z
                );
            }
        }
    }

    float GetHighestY()
    {
        float maxY = quads[0].position.y;

        foreach (Transform quad in quads)
        {
            if (quad.position.y > maxY)
            {
                maxY = quad.position.y;
            }
        }

        return maxY;
    }
}