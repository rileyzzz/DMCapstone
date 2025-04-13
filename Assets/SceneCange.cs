using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneCange : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    // This method can be linked to a UI Button OnClick() event
    public void ChangeScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("Scene name not set in inspector!");
        }
    }
}

