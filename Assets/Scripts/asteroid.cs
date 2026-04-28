using UnityEngine;

public class asteroid : MonoBehaviour
{
    public AsteroidData data;
    public float speed = 5.0f;
    private Rigidbody rb;
    Vector3 screenBounds;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb)
            rb.linearVelocity = new Vector3(0, -speed, 0);

        float depth = Mathf.Abs(Camera.main.transform.position.z);
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, depth));
        ApplyVisuals();

        if (rb)
            rb.angularVelocity = Random.insideUnitSphere * 2f;
    }

    private void ApplyVisuals()
    {
        if (!data)
            return;

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
        if (transform.position.y < screenBounds.y * -1)
        {
            Destroy(this.gameObject);
        }
    }

    void OnMouseDown()
    {
        ClawModule bestClaw = null;
        float bestDistanceSqr = float.MaxValue;

        var claws = FindObjectsByType<ClawModule>(FindObjectsInactive.Exclude);
        foreach (var candidate in claws)
        {
            if (!candidate || !candidate.IsAvailable)
                continue;

            float distanceSqr = (candidate.transform.position - transform.position).sqrMagnitude;
            if (distanceSqr >= bestDistanceSqr)
                continue;

            bestDistanceSqr = distanceSqr;
            bestClaw = candidate;
        }

        if (bestClaw)
            bestClaw.TryStartGrab(gameObject, data);
    }
}