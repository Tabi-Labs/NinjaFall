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
    [SerializeField] public CharacterSelectorHandler[] characterSelectorHandlers;
    [SerializeField] private CharacterData[] characterDatas;
    [SerializeField] private string previousScene = "MainMenuScene";
    [SerializeField] private string gameScene = "GameScene";
    [SerializeField] private GameObject GameStartBanner;
    private PlayerInputManager playerInputManager;
    public bool[] lockedCharacterData { get; private set; }
    public int MaxPlayers { get; private set; }

    // Esta propiedad actuará como proxy para usar NetworkReadyCount cuando estemos en red
    // o usar _localReadyCount cuando estemos en modo local
    public int readyCount
    {
        get => NetworkManager ? NetworkReadyCount.Value : _localReadyCount;
        private set
        {
            if (NetworkManager && IsServer)
            {
                // Solo el servidor puede modificar la variable de red directamente
                NetworkReadyCount.Value = value;
            }
            _localReadyCount = value;
        }
    }

    // Respaldo para modo local
    private int _localReadyCount = 0;

    // Variable para trackear qué paneles han sido asignados a cada cliente
    private Dictionary<ulong, int> clientPanelAssignments = new Dictionary<ulong, int>();

    // Singleton Pattern
    public static PlayerConfigurationManager Instance { get; private set; }

    // NetworkVariable para sincronizar el conteo de jugadores listos
    public NetworkVariable<int> NetworkReadyCount = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("[Singleton] Trying to instantiate a second instance of a singleton class.");
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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Inicializar NetworkReadyCount cuando se haga spawn del objeto en red
        if (IsServer)
        {
            NetworkReadyCount.Value = 0;
        }

        // Suscribirse al evento de cambio de la NetworkVariable
        if (IsClient)
        {
            NetworkReadyCount.OnValueChanged += OnNetworkReadyCountChanged;
        }

        Debug.Log($"NetworkSpawn - IsServer: {IsServer}, IsClient: {IsClient}, Ready Count: {NetworkReadyCount.Value}");
    }

    private void Start()
    {
        if (NetworkManager)
        {
            playerInputManager = GetComponent<PlayerInputManager>();
            playerInputManager.enabled = false;
            StartCoroutine(FindAndAssignPlayerToPanel());

            // Suscribirse al evento de conexión de clientes
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            }
        }
    }

    // Callback para cuando la variable de red cambia
    private void OnNetworkReadyCountChanged(int previousValue, int newValue)
    {
        Debug.Log($"ReadyCount changed from {previousValue} to {newValue}");

        // Actualizar el contador local para mantener sincronizado
        _localReadyCount = newValue;

        // Actualizar el banner según el nuevo valor
        UpdateGameStartBanner(newValue);
    }

    // Método para actualizar la visualización del banner
    private void UpdateGameStartBanner(int count)
    {
        if (count >= 2)
        {
            GameStartBanner.SetActive(true);
        }
        else
        {
            GameStartBanner.SetActive(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsClient)
        {
            NetworkReadyCount.OnValueChanged -= OnNetworkReadyCountChanged;
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse del evento al destruir el objeto
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    // Método público para actualizar el contador de jugadores listos desde el exterior
    public void UpdateReadyCount(int delta)
    {
        Debug.Log($"UpdateReadyCount called with delta: {delta}, IsServer: {IsServer}");

        if (NetworkManager)
        {
            if (IsServer)
            {
                // Solo el servidor puede modificar directamente la variable de red
                NetworkReadyCount.Value += delta;
                Debug.Log($"Server updated NetworkReadyCount to {NetworkReadyCount.Value}");
            }
            else
            {
                // Si no somos servidor, enviar RPC al servidor
                UpdateReadyCountServerRpc(delta);
                Debug.Log($"Client sent UpdateReadyCountServerRpc with delta: {delta}");
            }
        }
        else
        {
            // Modo local
            _localReadyCount += delta;
            UpdateGameStartBanner(_localReadyCount);
            Debug.Log($"Local mode updated _localReadyCount to {_localReadyCount}");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateReadyCountServerRpc(int delta)
    {
        // Solo el servidor procesa esta RPC
        if (!IsServer) return;

        // Actualizar la variable de red
        NetworkReadyCount.Value += delta;
        Debug.Log($"Server received UpdateReadyCountServerRpc, updated to {NetworkReadyCount.Value}");
    }

    // Se llama cuando un cliente se conecta
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}, IsServer: {IsServer}");

        if (IsServer)
        {
            // Sincronizar asignaciones de paneles actuales con el nuevo cliente
            foreach (var kvp in clientPanelAssignments)
            {
                SyncPanelAssignmentToClientRpc(kvp.Key, kvp.Value);
            }

            // Sincronizar el estado de los personajes bloqueados
            SyncLockedCharactersToClientRpc(clientId);
        }
    }

    [ClientRpc]
    private void SyncLockedCharactersToClientRpc(ulong clientId)
    {
        // Solo el cliente destinatario procesa esta RPC
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        Debug.Log($"Received SyncLockedCharactersToClientRpc for client {clientId}");

        // Actualizar el estado local de los personajes bloqueados
        for (int i = 0; i < lockedCharacterData.Length; i++)
        {
            if (lockedCharacterData[i])
            {
                Debug.Log($"Character {i} is already locked");
            }
        }
    }

    [ClientRpc]
    private void SyncPanelAssignmentToClientRpc(ulong ownerId, int panelIndex)
    {
        // Actualizar nuestra tabla local de asignaciones
        if (!clientPanelAssignments.ContainsKey(ownerId))
        {
            clientPanelAssignments[ownerId] = panelIndex;
            Debug.Log($"Received panel assignment for owner {ownerId} to panel {panelIndex}");
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

        ulong localClientId = NetworkManager.Singleton.LocalClientId;

        // Buscar un panel disponible
        CharacterSelectorHandler csh = null;
        int panelIndex = -1;

        // Si es el host, siempre asignar el primer panel
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

            // Registrar esta asignación
            clientPanelAssignments[localClientId] = panelIndex;

            // Informar a otros clientes sobre nuestra asignación
            if (IsServer)
            {
                RegisterPanelAssignmentServerRpc(localClientId, panelIndex);
            }
        }
        else
        {
            // Para clientes, solicitar un panel al servidor
            RequestPanelAssignmentServerRpc(localClientId);

            // Esperar hasta que recibamos la asignación
            float timeout = 5.0f; // 5 segundos máximo de espera
            float elapsed = 0f;

            while (!clientPanelAssignments.ContainsKey(localClientId) && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

            if (!clientPanelAssignments.ContainsKey(localClientId))
            {
                Debug.LogError("No se recibió asignación de panel del servidor en el tiempo esperado.");
                yield break;
            }

            panelIndex = clientPanelAssignments[localClientId];
            csh = characterSelectorHandlers[panelIndex];
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

    [ServerRpc(RequireOwnership = false)]
    private void RequestPanelAssignmentServerRpc(ulong clientId)
    {
        Debug.Log($"Received RequestPanelAssignmentServerRpc from client {clientId}");

        // Buscar el primer panel disponible a partir del índice 1
        int assignedPanel = -1;

        for (int i = 1; i < characterSelectorHandlers.Length; i++)
        {
            // Verificar si este panel ya está asignado a algún cliente
            if (!clientPanelAssignments.ContainsValue(i) && characterSelectorHandlers[i].isAvailable)
            {
                assignedPanel = i;
                break;
            }
        }

        if (assignedPanel == -1)
        {
            Debug.LogError($"No hay paneles disponibles para el cliente {clientId}");
            return;
        }

        // Registrar esta asignación
        RegisterPanelAssignmentServerRpc(clientId, assignedPanel);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RegisterPanelAssignmentServerRpc(ulong clientId, int panelIndex)
    {
        Debug.Log($"Received RegisterPanelAssignmentServerRpc from client {clientId} for panel {panelIndex}");

        // Registrar la asignación en el servidor
        clientPanelAssignments[clientId] = panelIndex;

        // Informar a todos los clientes sobre esta asignación
        SyncPanelAssignmentToClientRpc(clientId, panelIndex);
    }

    // Brief: Handle player join, called from PlayerInputManager
    // --------------------------------------------------------------------------------

    public void HandlePlayerJoin(PlayerInput pi)
    {
        if (NetworkManager) return;
        int i;
        CharacterSelectorHandler csh = null;

        // Get free CharacterSelectorHandler
        for (i = 0; i < characterSelectorHandlers.Length; i++)
        {
            if (characterSelectorHandlers[i].isAvailable)
            {
                csh = characterSelectorHandlers[i];
                break;
            }
        }

        MonoBehaviour[] components = pi.GetComponents<MonoBehaviour>();
        pi.transform.position = new Vector3(1000, 1000, 1000);
        foreach (MonoBehaviour component in components)
        {
            if (component is PlayerInput || component is NetworkObject || component is Player) continue;
            component.enabled = false;
        }
        pi.GetComponent<SpriteRenderer>().enabled = false;
        pi.GetComponent<Animator>().enabled = false;

        // Assign PlayerInput to CharacterSelectorHandler
        if (csh)
        {
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

        Debug.Log($"LockCharacter called for character {charIdx}");

        lockedCharacterData[charIdx] = true;
        UpdateReadyCount(1); // Usar el método que manejará la red

        pi.GetComponent<SpriteRenderer>().material = characterDatas[charIdx].mat;
        pi.GetComponent<Player>().CharacterData = characterDatas[charIdx];
        pi.GetComponent<Player>().isReady = true;

        return true;
    }

    public bool UnlockCharacter(int charIdx, PlayerInput pi)
    {
        if (charIdx < 0 || charIdx >= lockedCharacterData.Length)
        {
            Debug.LogError("charIdx out of range: " + charIdx);
            return false;
        }

        Debug.Log($"UnlockCharacter called for character {charIdx}");

        pi.GetComponent<Player>().isReady = false;

        UpdateReadyCount(-1); // Usar el método que manejará la red
        lockedCharacterData[charIdx] = false;
        return true;
    }

    public void StartGame()
    {
        if (readyCount < 2)
        {
            Debug.LogError("Not enough players to start the game.");
            return;
        }
        if(NetworkManager)
            NetworkManager.Singleton.SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
        else
            SceneLoader.Instance.ChangeScene(gameScene);
    }

    public void BackToMainMenu()
    {
        GameObject[] tagPlayer = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in tagPlayer)
        {
            Destroy(player);
        }
        if(NetworkManager)
            NetworkManager.Singleton.SceneManager.LoadScene(previousScene, LoadSceneMode.Single);
        else
            SceneLoader.Instance.ChangeScene(previousScene);
    }
}
