using UnityEngine;

public class BackToMenuButton : MonoBehaviour
{
    public void GoBackToMenu()
    {
        Debug.Log("BackToMenuButton: clicked");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveCurrentGame();
            GameManager.Instance.GoToMainMenu();
        }
        else
        {
            Debug.LogError("BackToMenuButton: GameManager.Instance is NULL");
        }
    }
}