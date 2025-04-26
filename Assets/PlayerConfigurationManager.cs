using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class PlayerConfigurationManager : NetworkBehaviour
{
    [SerializeField] public InputSystemUIInputModule inputModule;
    [SerializeField] private CharacterSelectorHandler[] characterSelectorHandlers;
    [SerializeField] private CharacterData[] characterDatas;
    [SerializeField] private string previousScene = "MainMenuScene";
    [SerializeField] private string gameScene = "GameScene";
    [SerializeField] private GameObject GameStartBanner;
    private PlayerInputManager playerInputManager;
    public bool[] lockedCharacterData{ get; private set; }
    public int MaxPlayers {get; private set;}
    public int readyCount {get; private set;} = 0;

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
    private void Start()
    {
        if(NetworkManager)
        {
            playerInputManager = GetComponent<PlayerInputManager>();
            playerInputManager.enabled = false;
            StartCoroutine(FindAndAssignPlayerToPanel());
        }
    }

    private IEnumerator FindAndAssignPlayerToPanel()
    {
        // Esperar un frame para asegurarnos de que todos los objetos están inicializados
        yield return null;

        // Buscar todos los Player en la escena
        Player[] allPlayers = FindObjectsOfType<Player>();
        Player localPlayer = null;
        foreach (Player player in allPlayers)
        {
            if (player.IsLocalPlayer)
            {
                localPlayer = player;
                break;
            }
        }
        // Si no encontramos un jugador local, salir
        if (localPlayer == null)
        {
            Debug.LogWarning("No se encontró un Player local en la escena.");
            yield break;
        }
        // Buscar un panel disponible
        CharacterSelectorHandler csh = null;
        int panelIndex = -1;

        // Si es el host, intentar asignar el primer panel
        if (NetworkManager.Singleton.IsHost)
        {
            csh = characterSelectorHandlers[0];
            panelIndex = 0;

            // Si ya está ocupado, liberarlo primero
            if (!csh.isAvailable)
            {
                Debug.Log("El panel P1 ya está ocupado, pero está reservado para el host. Liberando...");
                csh.Deactivate();
            }
        }
        else
        {
            // Para clientes, buscar un panel libre a partir del segundo panel (índice 1)
            for (int i = 1; i < characterSelectorHandlers.Length; i++)
            {
                if (characterSelectorHandlers[i].isAvailable)
                {
                    csh = characterSelectorHandlers[i];
                    panelIndex = i;
                    break;
                }
            }
        }
        // Si no se encontró un panel disponible
        if (csh == null)
        {
            Debug.LogWarning("No se encontró un panel disponible para el jugador local.");
            yield break;
        }
        Debug.Log($"Asignando Player local al panel {panelIndex}");
        // Obtener el PlayerInput del jugador
        PlayerInput playerInput = localPlayer.GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("El Player local no tiene un componente PlayerInput.");
            yield break;
        }

        // Desactivar componentes que no deberían interactuar con el mundo de juego
        MonoBehaviour[] components = playerInput.GetComponents<MonoBehaviour>();
        playerInput.transform.position = new Vector3(1000, 1000, 1000);
        foreach (MonoBehaviour component in components)
        {
            if (component is PlayerInput || component is NetworkObject) continue;
            component.enabled = false;
        }
        playerInput.GetComponent<SpriteRenderer>().enabled = false;
        playerInput.GetComponent<Animator>().enabled = false;

        // Asignar el panel al jugador
        playerInput.uiInputModule = csh.GetComponentInChildren<InputSystemUIInputModule>();
        csh.Activate(playerInput);

        Debug.Log($"Player local asignado exitosamente al panel {panelIndex}");
    }
    // Brief: Handle player join, called from PlayerInputManager
    // --------------------------------------------------------------------------------

    public void HandlePlayerJoin(PlayerInput pi)
    {
        if (NetworkManager) return;
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

        MonoBehaviour[] components = pi.GetComponents<MonoBehaviour>();
        pi.transform.position= new Vector3(1000,1000,1000);
        foreach (MonoBehaviour component in components)
        {
            if (component is PlayerInput || component is NetworkObject || component is Player) continue;
            component.enabled = false;
        }
        pi.GetComponent<SpriteRenderer>().enabled = false;
        pi.GetComponent<Animator>().enabled = false;

        // Assign PlayerInput to CharacterSelectorHandler
        if(csh){
            pi.uiInputModule = csh.GetComponentInChildren<InputSystemUIInputModule>();
            csh.Activate(pi);
        }
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
}