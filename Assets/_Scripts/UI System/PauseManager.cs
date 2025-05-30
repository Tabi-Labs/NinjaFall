using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using Unity.Netcode;

public enum PauseMode
{
    pre_game,
    mid_game,
    post_game
}

public class PauseManager : NetworkBehaviour
{
    [Header("Pause Settings")]
    [SerializeField] PauseMode _initialPauseMode = PauseMode.pre_game;
    [SerializeField] bool _startPaused = true;
    [Header("UI Elements")]
    [SerializeField] GameObject start_canvas;
    [SerializeField] GameObject pause_canvas;
    [SerializeField] GameObject finish_canvas;
    [SerializeField] GameObject winner_portrait;
    [SerializeField] TextMeshProUGUI winner_text;
    
    private CharacterData character_data;

    private bool is_paused = false;
    private bool ignore_pause = false;
    private int last_winner = -1;

    // Singleton Pattern
    // --------------------------------------------------------------------------------
    public static PauseManager instance { get; private set; }
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    public void PauseFunctionality(bool pause, PauseMode mode)
    {       
        switch (mode) {
            case PauseMode.pre_game:
                if (NetworkManager && IsServer)
                {
                    PreGameClientRpc(pause);
                }
                else
                {
                    PreGameFunctionality(pause);
                }

                break;
            case PauseMode.mid_game:
                if (NetworkManager && IsServer)
                {
                    MidGameClientRpc(pause);
                }
                else
                {
                    MidGameFunctionality(pause);
                }
                break;
            case PauseMode.post_game:
                if(NetworkManager && IsServer)
                {
                    PostGameClientRpc();
                }
                else
                {
                    PostGameFunctionality();
                }
                break;
        }
    }

    private IEnumerator EndGameAnimation()
    {
        winner_portrait.GetComponent<Image>().sprite = character_data.portrait;
        Animator anim = winner_portrait.GetComponent<Animator>();
        anim.runtimeAnimatorController = character_data.portraitAnimator;
        anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        winner_portrait.transform.GetChild(0).GetComponent<Image>().sprite = character_data.text;
        winner_text.text = character_data.victoryPhrases[Random.Range(0, character_data.victoryPhrases.Length)];

        finish_canvas.SetActive(true);
        Image background = finish_canvas.transform.GetChild(0).GetComponent<Image>();
        Image overlay = finish_canvas.transform.GetChild(1).GetComponent<Image>();

        background.gameObject.SetActive(false);

        overlay.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        yield return overlay.DOFade(0.0f, 0.2f)
                    .SetLoops(3, LoopType.Yoyo)
                    .SetEase(Ease.OutSine)
                    .WaitForCompletion();

        AudioManager.PlayMusic("MX_Win");
        yield return new WaitForSeconds(0.5f);

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player"); //Puede cambiar
        foreach (GameObject player in players)
        {
            if (player.GetComponent<CustomInputManager>() != null)
            {
                player.GetComponent<CustomInputManager>().enabled = false;
            }
        }

        background.gameObject.SetActive(true);
        overlay.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        overlay.DOFade(0.0f, 2.0f).SetEase(Ease.OutSine)
            .OnComplete(()=>overlay.gameObject.SetActive(false));
    }

    public void StartGame()
    {
        PauseFunctionality(_startPaused, _initialPauseMode);
    }
    
    public void PauseGame()
    {
        if (ignore_pause) return;
        ignore_pause = true;
        StartCoroutine(allow_pause());
        PauseFunctionality(!is_paused, PauseMode.mid_game);
    }
    
    IEnumerator allow_pause()
    {
        yield return new WaitForSeconds(0.5f);
        ignore_pause = false;
    }
    private void StopPlayers(bool pause)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player"); //Puede cambiar

        foreach (GameObject player in players)
        {
            if (player.GetComponent<CustomInputManager>() != null)
            {
                player.GetComponent<CustomInputManager>().enabled = !pause;
            }
        }
    }
    private void PreGameFunctionality(bool pause)
    {
        if(NetworkManager && IsServer)
        {
            StopPlayersClientRpc(pause);
        }
        else
        {
            StopPlayers(pause);
        }

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
    }
    private void MidGameFunctionality(bool pause)
    {

        if(NetworkManager && IsServer)
        {
            StopPlayersClientRpc(pause);
        }
        else
        {
            StopPlayers(pause);
        }

        Time.timeScale = pause ? 0.0f : 1.0f;
        pause_canvas.SetActive(pause);
        is_paused = pause;
    }
    private void PostGameFunctionality()
    {
        // �apa de la las gordas
        if(!NetworkManager)
            BotManager.Instance.StopBots();
        StartCoroutine(EndGameAnimation());
    }
    [Rpc(SendTo.Everyone)]
    private void StopPlayersClientRpc(bool pause)
    {
        StopPlayers(pause);
    }
    [Rpc(SendTo.Everyone)]
    private void ResumePlayersClientRpc()
    {
        Resume();
    }
    [Rpc(SendTo.Everyone)]
    private void PreGameClientRpc(bool pause)
    {
        PreGameFunctionality(pause);
    }
    [Rpc(SendTo.Everyone)]
    private void MidGameClientRpc(bool pause)
    {
        MidGameFunctionality(pause);
    }
    [Rpc(SendTo.Everyone)]
    private void PostGameClientRpc()
    {
        PostGameFunctionality();
    }
    public void EndGame(CharacterData winnerData)
    {
        character_data = winnerData;
        PauseFunctionality(true, PauseMode.post_game);
    }

    public void ClearPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (BotManager.Instance != null && BotManager.Instance.bots != null) {
            BotManager.Instance.CleanupBots();
        }

        foreach (GameObject player in players)
        {
            Destroy(player);
        }
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
        if (NetworkManager)
        {
            ResumePlayersClientRpc();
        }
        else
        {
            Resume();   
        }
           
    }
    private void Resume()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (player.GetComponent<CustomInputManager>() != null)
            {
                player.GetComponent<CustomInputManager>().enabled = true;
            }
        }

        pause_canvas.SetActive(false);
        Time.timeScale = 1.0f;
    }
    //TODO! - Put this function in a better place
    public void ErasePlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            Destroy(player);
        }
    }

}
