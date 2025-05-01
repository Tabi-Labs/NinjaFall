using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

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

    // Variables for handling bot addition
    public PlayerInput hostPlayerInput  {get; private set;} = null;
    private int newBotId = -1;
    public bool isAddingBot {get; private set;} = false;
    public bool preventBotAddition {get; private set;} = true;
    private InputSystemUIInputModule hostInputModule = null;
    private InputAction addBotAction;

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

        // Handle case when no free CharacterSelectorHandler is available
        if(csh == null)
        {
            Debug.Log("No free CharacterSelectorHandler available.");
            if(isAddingBot)
            {
                isAddingBot = false;
                BotManager.Instance.RemoveBotById(newBotId);
            }
            Destroy(pi.gameObject);
            return;
        }

        // Handle case when player is host
        if(pi.playerIndex == 0){
            StartCoroutine(DelayAddBotAction(pi));
            hostPlayerInput = pi;
            hostInputModule = csh.GetComponentInChildren<InputSystemUIInputModule>();
            preventBotAddition = false;
        }

        // Disable components except PlayerInput on player prefab
        MonoBehaviour[] components = pi.GetComponents<MonoBehaviour>();
        pi.transform.position= new Vector3(1000,1000,1000);
        foreach (MonoBehaviour component in components)
        {
            if (component is PlayerInput) continue;
            component.enabled = false;
        }
        pi.GetComponent<SpriteRenderer>().enabled = false;
        pi.GetComponent<Animator>().enabled = false;

        // Assign PlayerInput to CharacterSelectorHandler
        if(isAddingBot){
            hostPlayerInput.uiInputModule = csh.GetComponentInChildren<InputSystemUIInputModule>();
        } else {
            pi.uiInputModule = csh.GetComponentInChildren<InputSystemUIInputModule>();
        }

        csh.Activate(pi, isAddingBot);
    }

    public void HandleBotJoin(InputAction.CallbackContext context)
    {
        if (preventBotAddition) return;
        isAddingBot = true;
        preventBotAddition = true;
        newBotId = BotManager.Instance.CreateBot();
    }

    public void RestoreHostInput(){
        SwitchPlayerUI(hostPlayerInput, hostInputModule);
        hostInputModule = hostPlayerInput.uiInputModule;
        preventBotAddition = false;
    }

    // Getters and Setters
    // --------------------------------------------------------------------------------

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

    public bool LockCharacter(int charIdx, PlayerInput pi)
    {
        if (charIdx < 0 || charIdx >= lockedCharacterData.Length)
        {
            Debug.LogError("charIdx out of range: " + charIdx);
            return false;
        }

        lockedCharacterData[charIdx] = true;
        readyCount++;

        pi.GetComponent<SpriteRenderer>().material = characterDatas[charIdx].mat;
        pi.GetComponent<Player>().CharacterData = characterDatas[charIdx];
        pi.GetComponent<Player>().isReady = true;

        if(readyCount >= 2) GameStartBanner.SetActive(true);
        return true;
    }

    public bool UnlockCharacter(int charIdx, PlayerInput pi)
    {
        if (charIdx < 0 || charIdx >= lockedCharacterData.Length)
        {
            Debug.LogError("charIdx out of range: " + charIdx);
            return false;
        }

        pi.GetComponent<Player>().isReady = false;

        readyCount--;
        if(readyCount <= 1) GameStartBanner.SetActive(false);
        lockedCharacterData[charIdx] = false;
        return true;
    }

    public void StartGame()
    {
        if (readyCount < 2){
            Debug.LogError("Not enough players to start the game.");
            return;
        }
        SceneLoader.Instance.ChangeScene(gameScene);
    }

    public void BackToMainMenu()
    {
        GameObject[] tagPlayer = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in tagPlayer)
        {
            Destroy(player);
        }
        SceneLoader.Instance.ChangeScene(previousScene);
    }

    private InputSystemUIInputModule ResetInputModule(InputSystemUIInputModule originalModule)
    {
        if (originalModule == null)
        {
            Debug.LogError("[ResetInputModule] Original module is null.");
            return null;
        }

        GameObject parentGameObject = originalModule.gameObject;

        // Backup any important fields if needed (before destroying)
        var actionsAsset = originalModule.actionsAsset;
        var pointAction = originalModule.point;
        var moveAction = originalModule.move;
        var submitAction = originalModule.submit;
        var cancelAction = originalModule.cancel;
        var leftClickAction = originalModule.leftClick;
        var middleClickAction = originalModule.middleClick;
        var rightClickAction = originalModule.rightClick;
        var scrollWheelAction = originalModule.scrollWheel;
        var trackedDevicePositionAction = originalModule.trackedDevicePosition;
        var trackedDeviceOrientationAction = originalModule.trackedDeviceOrientation;

        // Destroy the old InputModule
        DestroyImmediate(originalModule);

        // Create a fresh one
        InputSystemUIInputModule newModule = parentGameObject.AddComponent<InputSystemUIInputModule>();

        // Reassign the fields
        newModule.actionsAsset = actionsAsset;
        newModule.point = pointAction;
        newModule.move = moveAction;
        newModule.submit = submitAction;
        newModule.cancel = cancelAction;
        newModule.leftClick = leftClickAction;
        newModule.middleClick = middleClickAction;
        newModule.rightClick = rightClickAction;
        newModule.scrollWheel = scrollWheelAction;
        newModule.trackedDevicePosition = trackedDevicePositionAction;
        newModule.trackedDeviceOrientation = trackedDeviceOrientationAction;

        // Enable the module
        newModule.enabled = true;
        newModule.UpdateModule();

        return newModule;
    }

    public void SwitchPlayerUI(PlayerInput playerInput, InputSystemUIInputModule newModule)
    {
        GameObject newModuleGO = newModule.gameObject;
        newModule.enabled = false;
        Destroy(newModule);
        InputSystemUIInputModule newModuleInstance = newModuleGO.AddComponent<InputSystemUIInputModule>();
        newModuleInstance.actionsAsset = playerInput.actions;
        playerInput.uiInputModule = newModuleInstance;
        playerInput.uiInputModule.enabled = true;
        playerInput.uiInputModule.UpdateModule();
    }

    // Auxiliary Functions
    // --------------------------------------------------------------------------------

    private IEnumerator DelayAddBotAction(PlayerInput pi)
    {
        yield return null;
        addBotAction = pi.actions.FindAction("AddBot");
        addBotAction.performed += HandleBotJoin;
        addBotAction.Enable();
    }
}