using UnityEngine;

public class StarFlicker : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private float flickerSpeed = 2f;
    [SerializeField] private float minAlpha = 0.65f;
    [SerializeField] private float maxAlpha = 1f;

    private Material materialInstance;
    private Color baseColor;

    void Start()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        if (targetRenderer == null)
        {
            Debug.LogWarning($"StarFlicker on {gameObject.name} has no Renderer assigned or found.");
            enabled = false;
            return;
        }

        materialInstance = targetRenderer.material;
        baseColor = materialInstance.color;
    }

    void Update()
    {
        if (materialInstance == null) return;

        float t = (Mathf.Sin(Time.time * flickerSpeed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        Color c = baseColor;
        c.a = alpha;
        materialInstance.color = c;
    }
}