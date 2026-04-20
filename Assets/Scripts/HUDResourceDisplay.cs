using TMPro;
using UnityEngine;

public class HUDResourceDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text adText;
    [SerializeField] private TMP_Text scText;
    [SerializeField] private TMP_Text sfText;
    [SerializeField] private TMP_Text nText;

    void Update()
    {
        if (GameManager.Instance == null) return;

        adText.text = GameManager.Instance.GetStock(MaterialTier.AsteroidDust).ToString();
        scText.text = GameManager.Instance.GetStock(MaterialTier.SpaceCrystal).ToString();
        sfText.text = GameManager.Instance.GetStock(MaterialTier.StarFragment).ToString();
        nText.text = GameManager.Instance.GetStock(MaterialTier.Novaflare).ToString();
    }
}