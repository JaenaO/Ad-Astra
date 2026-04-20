using UnityEngine;

public class SceneSwapButton : MonoBehaviour
{
    public void SwapScene()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SwapGameplayScene();
        }
        else
        {
            Debug.LogError("GameManager.Instance is null.");
        }
    }
}