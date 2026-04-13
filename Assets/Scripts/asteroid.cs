using UnityEngine;

public class asteroid : MonoBehaviour
{
    public AsteroidData data;
    public float speed = 5.0f;
    private Rigidbody rb;
    Vector3 screenBounds;

    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        rb.linearVelocity = new Vector3(0, -speed, 0);
        float depth = Mathf.Abs(Camera.main.transform.position.z);
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, depth));
            ApplyVisuals();

        // Apply rarity visuals if data is assigned
        if (data != null)
        {
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.material = new Material(renderer.material);
                renderer.material.color = data.color;
            }
            transform.localScale = data.scale;
        }
        rb.angularVelocity = Random.insideUnitSphere * 2f;

        void ApplyVisuals()
{
    if (data != null)
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            // Create instance properly for URP
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", data.color);
            renderer.SetPropertyBlock(block);
        }
        transform.localScale = data.scale;
    }
}
    }

    void Update()
    {
        if (transform.position.y < screenBounds.y * -1)
        {
            Destroy(this.gameObject);
        }
    }

    void OnMouseDown()
    {
        int credits = (data != null) ? data.creditValue : 10;
        GameManager.Instance.AddCredits(credits);
        Destroy(gameObject);
    }
}