using UnityEngine;

public class MainMenuButton : MonoBehaviour
{
    public enum ButtonAction
    {
        NewGame,
        LoadPreviousGame,
        Settings,
        Exit
    }

    [SerializeField] private ButtonAction action;
    [SerializeField] private GameObject noSavePopup;
    [SerializeField] private GameObject settingsPanel;

    public void OnButtonPressed()
    {
        switch (action)
        {
            case ButtonAction.NewGame:
                StartNewGame();
                break;

            case ButtonAction.LoadPreviousGame:
                LoadPreviousGame();
                break;

            case ButtonAction.Settings:
                OpenSettings();
                break;

            case ButtonAction.Exit:
                ExitGame();
                break;
        }
    }

    private void StartNewGame()
    {
        Debug.Log("MainMenuButton: StartNewGame clicked");

        SaveManager.NewGame();

        if (GameManager.Instance != null)
        {
            Debug.Log("MainMenuButton: GameManager found");
            GameManager.Instance.ResetForNewGame();
            GameManager.Instance.StartNewGameScene();
        }
        else
        {
            Debug.LogError("MainMenuButton: GameManager.Instance is NULL");
        }
    }

    private void LoadPreviousGame()
    {
        if (SaveManager.HasSave())
        {
            SaveManager.LoadGame();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadSavedGameIntoManager();
                GameManager.Instance.LoadPreviousGameScene();
            }
        }
        else
        {
            if (noSavePopup != null)
            {
                PopupSlide popup = noSavePopup.GetComponent<PopupSlide>();
                if (popup != null)
                    popup.ShowPopup();
                else
                    noSavePopup.SetActive(true);
            }

            Debug.Log("No previous save found.");
        }
    }

    private void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}