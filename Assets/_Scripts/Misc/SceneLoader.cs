using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class SceneLoader : MonoBehaviour
{
    [SerializeField]
    Image transitionCourtain;
    [SerializeField]
    float transitionDuration = 0.5f;

    // Singleton instance
    public static SceneLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to change scene by name
    public void ChangeScene(string sceneName)
    {
        StartCoroutine(LoadSceneWithTransition(sceneName));
    }

    private IEnumerator LoadSceneWithTransition(string sceneName)
    {
        if (transitionCourtain != null)
        {
            yield return transitionCourtain.DOFade(1, transitionDuration).WaitForCompletion();
        }

        SceneManager.LoadScene(sceneName);

        if (transitionCourtain != null)
        {
            yield return transitionCourtain.DOFade(0, transitionDuration).WaitForCompletion();
        }
    }
}