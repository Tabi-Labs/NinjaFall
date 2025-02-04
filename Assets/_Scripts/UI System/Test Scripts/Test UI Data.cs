
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class TestUIData : MonoBehaviour
{
    public string mainMenuScene = "Main Menu";
    public SceneAsset MainMenuScene;

    public void LoadScene(SceneAsset scene)
    {
        if(Application.IsPlaying(this))
            SceneManager.LoadScene(scene.name);
        else
            EditorSceneManager.OpenScene($"Assets/Scenes/{scene.name}.unity");
    }
}
