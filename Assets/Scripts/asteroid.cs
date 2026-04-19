using UnityEngine;

public class asteroid : MonoBehaviour
{
    public AsteroidData data;
    public float speed = 5.0f;
    private Rigidbody rb;
    private Vector3 screenBounds;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = new Vector3(0, -speed, 0);
        rb.angularVelocity = Random.insideUnitSphere * 2f;

        float depth = Mathf.Abs(Camera.main.transform.position.z);
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, depth));

        ApplyVisuals();
    }

    void ApplyVisuals()
    {
        if (data == null) return;

        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", data.color);
            renderer.SetPropertyBlock(block);
        }
        transform.localScale = data.scale;
    }

    void Update()
    {
        if (transform.position.y < -screenBounds.y)
            Destroy(gameObject);
    }

    void OnMouseDown()
    {
        if (data != null)
            data.DropLoot(transform.position);
        else
            GameManager.Instance.AddCredits(10);

        Destroy(gameObject);
    }
}