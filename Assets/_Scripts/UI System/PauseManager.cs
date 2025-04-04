using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [SerializeField] GameObject pause_canvas;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Placeholder, esto habra que llamarlo desde el input Manager
        if(Input.GetKeyDown(KeyCode.V)){
            PauseOfflineGame();
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
