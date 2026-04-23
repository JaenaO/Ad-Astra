using System.Collections;
using UnityEngine;

public class PopupSlide : MonoBehaviour
{
    [Header("Positions")]
    [SerializeField] private RectTransform panel;
    [SerializeField] private Vector2 hiddenPosition = new Vector2(-480f, 200f);
    [SerializeField] private Vector2 shownPosition = new Vector2(-480f, -200f);

    [Header("Timing")]
    [SerializeField] private float slideInTime = 0.35f;
    [SerializeField] private float bounceTime = 0.12f;
    [SerializeField] private float visibleTime = 5f;
    [SerializeField] private float slideOutTime = 0.3f;

    [Header("Bounce")]
    [SerializeField] private float bounceOffset = 20f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (panel == null)
            panel = GetComponent<RectTransform>();

        panel.anchoredPosition = hiddenPosition;
        
    }

    public void ShowPopup()
    {
        gameObject.SetActive(true);

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(PopupRoutine());
    }

    private IEnumerator PopupRoutine()
    {
        panel.anchoredPosition = hiddenPosition;

        // Slide down
        yield return Move(panel, hiddenPosition, shownPosition, slideInTime);

        // Small bounce
        Vector2 bounceDown = shownPosition + new Vector2(0f, bounceOffset);
        yield return Move(panel, shownPosition, bounceDown, bounceTime);
        yield return Move(panel, bounceDown, shownPosition, bounceTime);

        // Stay visible
        yield return new WaitForSeconds(visibleTime);

        // Slide back up
        yield return Move(panel, shownPosition, hiddenPosition, slideOutTime);

        gameObject.SetActive(false);
        currentRoutine = null;
    }

    private IEnumerator Move(RectTransform target, Vector2 start, Vector2 end, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // smooth easing
            t = Mathf.SmoothStep(0f, 1f, t);

            target.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        target.anchoredPosition = end;
    }
}