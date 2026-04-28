using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonDebug : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
{
    private RectTransform rt;
    private Image img;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        img = GetComponent<Image>();
    }

    void LogState(string label)
    {
        Debug.Log(
            $"{label} | Pos={rt.anchoredPosition} Size={rt.sizeDelta} " +
            $"Sprite={(img.sprite != null ? img.sprite.name : "null")} " +
            $"Override={(img.overrideSprite != null ? img.overrideSprite.name : "null")}"
        );
    }

    public void OnPointerEnter(PointerEventData eventData) => LogState("PointerEnter");
    public void OnPointerExit(PointerEventData eventData) => LogState("PointerExit");
    public void OnPointerClick(PointerEventData eventData) => LogState("PointerClick");
    public void OnSelect(BaseEventData eventData) => LogState("Select");
    public void OnDeselect(BaseEventData eventData) => LogState("Deselect");
}