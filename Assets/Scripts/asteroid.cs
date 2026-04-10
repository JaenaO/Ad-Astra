using UnityEngine;

public class asteroid : MonoBehaviour
{
    public float speed = 5.0f; // Speed at which the asteroid moves downwards
    private Rigidbody rb;
    Vector3 screenBounds;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        rb.linearVelocity = new Vector3(0, -speed, 0); // Set the velocity to move downwards from top to bottom of the screen
        float depth = Mathf.Abs(Camera.main.transform.position.z);
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, depth));
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < screenBounds.y * -1) // If the asteroid goes below the bottom of the screen
        {
            Destroy(this.gameObject); // Destroy the asteroid
        }
    }

    // i added this for the clickable asteroid- gives 10 credits (katy)
    void OnMouseDown()
{
    GameManager.Instance.AddCredits(10);
    Destroy(gameObject);
}
}
