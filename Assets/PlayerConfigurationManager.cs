using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class PlayerConfigurationManager : MonoBehaviour
{
    [SerializeField] public InputSystemUIInputModule inputModule;
    [SerializeField] private CharacterSelectorHandler[] characterSelectorHandlers;
    [SerializeField] private CharacterData[] characterDatas;
    [SerializeField] private string previousScene = "MainMenuScene";
    [SerializeField] private string gameScene = "GameScene";
    [SerializeField] private GameObject GameStartBanner;
    public bool[] lockedCharacterData{ get; private set; }
    public int MaxPlayers {get; private set;}
    public int readyCount {get; private set;} = 0;
    public int playerCount {get; private set;} = 0;
    private List<PlayerConfiguration> playerConfigs;

    // Singleton Pattern
    // --------------------------------------------------------------------------------

    public static PlayerConfigurationManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.Log("[Singleton] Trying to instantiate a seccond instance of a singleton class.");
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
            playerConfigs = new List<PlayerConfiguration>();
        }
        lockedCharacterData = new bool[characterDatas.Length];
        for (int i = 0; i < lockedCharacterData.Length; i++)
        {
            lockedCharacterData[i] = false;
        }
        MaxPlayers = characterSelectorHandlers.Length;
    }

    // Brief: Handle player join, called from PlayerInputManager
    // --------------------------------------------------------------------------------

    public void HandlePlayerJoin(PlayerInput pi)
    {
        int i;
        CharacterSelectorHandler csh = null;

        // Get free CharacterSelectorHandler
        for(i = 0; i < characterSelectorHandlers.Length; i++)
        {
            if(characterSelectorHandlers[i].isAvailable)
            {
                csh = characterSelectorHandlers[i];
                break;
            }
        }

        // Assign PlayerInput to CharacterSelectorHandler
        if(csh){
            pi.transform.SetParent(csh.transform);
            pi.uiInputModule = csh.GetComponentInChildren<InputSystemUIInputModule>();
            csh.Activate(playerCount++);
        }
    }

    // Getters and Setters
    // --------------------------------------------------------------------------------

    public List<PlayerConfiguration> GetPlayerConfigs()
    {
        return playerConfigs;
    }

    public void SetPlayerColor(int index, Material color)
    {
        playerConfigs[index].playerMaterial = color;
    }

    public bool GetCharacterData(int currentIndex, int direction, out CharacterData data, out int newIndex, out bool isLocked)
    {
        int index = (currentIndex + direction + characterDatas.Length) % characterDatas.Length;
        if (index < 0 || index >= characterDatas.Length)
        {
            Debug.LogError("Index out of range: " + index);
            data = null;
            isLocked = false;
            newIndex = -1;
            return false;
        }

        data = characterDatas[index];
        isLocked = lockedCharacterData[index];
        newIndex = index;
        return true;
    }

    public bool LockCharacter(int index)
    {
        if (index < 0 || index >= lockedCharacterData.Length)
        {
            Debug.LogError("Index out of range: " + index);
            return false;
        }

        lockedCharacterData[index] = true;
        readyCount++;
        if(readyCount >= 2) GameStartBanner.SetActive(true);
        return true;
    }

    public bool UnlockCharacter(int index)
    {
        if (index < 0 || index >= lockedCharacterData.Length)
        {
            Debug.LogError("Index out of range: " + index);
            return false;
        }

        readyCount--;
        if(readyCount <= 1) GameStartBanner.SetActive(false);
        lockedCharacterData[index] = false;
        return true;
    }

    public void StartGame()
    {
        if (playerCount < 2){
            Debug.LogError("Not enough players to start the game.");
            return;
        }
        SceneLoader.Instance.ChangeScene(gameScene);
        PlayerSpawner.Instance.SpawnPlayers(playerConfigs);
    }

    public void BackToMainMenu()
    {
        SceneLoader.Instance.ChangeScene(previousScene);
    }
}

// Auxiliary Class
// --------------------------------------------------------------------------------
public class PlayerConfiguration
{
    public PlayerConfiguration(PlayerInput pi)
    {
        PlayerIndex = pi.playerIndex;
        Input = pi;
    }

    public PlayerInput Input { get; private set; }
    public int PlayerIndex { get; private set; }
    public bool isReady { get; set; }
    public Material playerMaterial {get; set;}
}