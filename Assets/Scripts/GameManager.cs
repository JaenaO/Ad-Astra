using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int credits = 0;
    public TMP_Text creditsText; // drag your UI text here in Inspector

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddCredits(int amount)
    {
        credits += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (creditsText != null)
            creditsText.text = $"Credits: {credits}";
    }
}