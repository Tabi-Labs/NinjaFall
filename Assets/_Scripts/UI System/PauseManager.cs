using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public enum PauseMode
{
    pre_game,
    mid_game,
    post_game
}

public class PauseManager : MonoBehaviour
{
    [SerializeField] GameObject start_canvas;
    [SerializeField] GameObject pause_canvas;
    [SerializeField] GameObject finish_canvas;
    [SerializeField] GameObject winner_portrait;
    [SerializeField] private CharacterData[] character_data;

    private bool is_paused = false;
    private int last_winner = -1;
    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        //Placeholder, esto habra que llamarlo desde el input Manager
        if (Input.GetKeyDown(KeyCode.V))
        {
            //PauseOfflineGame();
            PauseGame();
        }
        else if (Input.GetKeyDown(KeyCode.P)) {
            EndGame(3);
        }

    }

    public void PauseFunctionality(bool pause, PauseMode mode)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player"); //Puede cambiar

        foreach (GameObject player in players)
        {
            if (player.GetComponent<CustomInputManager>() != null)
            {
                player.GetComponent<CustomInputManager>().enabled = pause;
            }

        }

        switch (mode) {
            case PauseMode.pre_game:
                Time.timeScale = 1.0f;

                start_canvas.SetActive(true);
                TextMeshProUGUI start_text = start_canvas.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                start_text.text = "READY";
                start_text.alpha = 1.0f;
                AudioManager.PlaySpeech("VO_Ready");
                start_text.DOFade(0.0f, 1.0f).OnComplete(() => {
                    start_text.text = "SET";
                    start_text.alpha = 1.0f;
                    AudioManager.PlaySpeech("VO_Set");
                    start_text.DOFade(0.0f, 1.0f).OnComplete(() => {
                        start_text.text = "GO!";
                        start_text.alpha = 1.0f;
                        AudioManager.PlaySpeech("VO_Go");
                        start_text.gameObject.transform.DOScale(new Vector3(1.3f, 1.3f, 1.0f), 0.5f).OnComplete(() => {
                            start_text.alpha = 0.0f;
                            start_canvas.SetActive(false);
                            PauseFunctionality(false, PauseMode.mid_game);
                        });
                    });
                });

                break;
            case PauseMode.mid_game:
                Time.timeScale = pause ? 0.0f : 1.0f;
                pause_canvas.SetActive(pause);
                is_paused = pause;
                break;
            case PauseMode.post_game:
                Time.timeScale = 1.0f;

                winner_portrait.GetComponent<Image>().sprite = character_data[last_winner].portrait;
                winner_portrait.GetComponent<Animator>().runtimeAnimatorController = character_data[last_winner].portraitAnimator;
                winner_portrait.transform.GetChild(0).GetComponent<Image>().sprite = character_data[last_winner].text;

                finish_canvas.SetActive(true);
                break;
        }
    }

    public void StartGame()
    {
        PauseFunctionality(true, PauseMode.pre_game);
    }

    public void PauseGame()
    {
        PauseFunctionality(!is_paused, PauseMode.mid_game);
    }
    public void EndGame(int winner)
    {
        last_winner = winner;
        PauseFunctionality(true, PauseMode.post_game);
    }

    public void PauseOfflineGame()
    {
        
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player"); //Puede cambiar

        foreach (GameObject player in players)
        {
            if (player.GetComponent<CustomInputManager>() != null) {
                player.GetComponent<CustomInputManager>().enabled = false;
            }
            
        }
    
        Time.timeScale = 0.0f;
        pause_canvas.SetActive(true);
    }

    public void ResumeOfflineGame()
    {
        
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (player.GetComponent<CustomInputManager>() != null) {
                player.GetComponent<CustomInputManager>().enabled = true;
            }
        }
    
        pause_canvas.SetActive(false);
        Time.timeScale = 1.0f;
    }
}
