using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour
{
    [SerializeField] private SceneAsset _sceneToLoad;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            Debug.LogWarning("Player is being destroyed and exiting to main menu. Review this code to ensure" + 
            "it is the intended behavior.");
            Destroy(other.transform.root.gameObject);
            ExitToMainMenu();
        }
    }
    private void ExitToMainMenu()
    {
        if(SceneLoader.Instance) SceneLoader.Instance.ChangeScene(_sceneToLoad.name);
        else SceneManager.LoadScene(_sceneToLoad.name);
        
    }
}
