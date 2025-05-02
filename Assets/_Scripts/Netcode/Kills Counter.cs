using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Linq;

public class KillsCounter : NetworkBehaviour
{
    public static KillsCounter Instance;
    private const int MAX_LIVES = 5;

    // Array to store player lives locally
    private int[] playerLives;

    // References to existing TextMeshProUGUI in the scene
    [SerializeField] private TextMeshProUGUI P1TextMesh;
    [SerializeField] private TextMeshProUGUI P2TextMesh;
    [SerializeField] private TextMeshProUGUI P3TextMesh;
    [SerializeField] private TextMeshProUGUI P4TextMesh;

    private TextMeshProUGUI[] playerTextMeshes;
    private int[] localLives = new int[4] { MAX_LIVES, MAX_LIVES, MAX_LIVES, MAX_LIVES };
    public bool[] alivePlayers;

    // Variable for local mode, count of local players
    private int localPlayerCount;

    // Flag to check if the game has ended
    private bool gameEnded = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Initialize for local or network mode
        if (!NetworkManager)
        {
            localPlayerCount = PlayerConfigurationManager.Instance.readyCount;

            // If no players detected, try from PlayerConfigurationManager
            if (localPlayerCount == 0 && PlayerConfigurationManager.Instance != null)
            {
                localPlayerCount = PlayerConfigurationManager.Instance.readyCount;
            }

            // Ensure we have at least one player to avoid errors
            if (localPlayerCount == 0)
            {
                localPlayerCount = 1;
                Debug.LogWarning("No players detected, setting localPlayerCount to 1");
            }

            Debug.Log($"KillsCounter: Initializing in local mode with {localPlayerCount} players");
            InitializeLocalMode();

            // Generate TextMeshPro in UI and update UI
            GeneratePlayerTextMeshes();
            UpdateUI();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Initialize the player lives array based on connected clients
        int playerCount = NetworkManager.ConnectedClientsIds.Count;
        playerLives = new int[playerCount];

        // Initialize all players with MAX_LIVES
        for (int i = 0; i < playerCount; i++)
        {
            playerLives[i] = MAX_LIVES;
        }

        // Initialize alivePlayers array
        alivePlayers = new bool[playerCount];
        for (int i = 0; i < alivePlayers.Length; i++)
        {
            alivePlayers[i] = true;
        }

        Debug.Log($"Initialized in network mode with {playerCount} players");

        // Generate TextMeshPro in UI
        GeneratePlayerTextMeshes();

        // If this is the server, synchronize initial lives to all clients
        if (IsServer)
        {
            SyncLivesToClientRpc(playerLives);
        }

        // Update the UI based on initial player lives
        UpdateUI();
    }

    private void InitializeLocalMode()
    {
        // Initialize local lives array
        localLives = new int[localPlayerCount];
        for (int i = 0; i < localPlayerCount; i++)
        {
            localLives[i] = MAX_LIVES;
        }

        // Initialize alivePlayers for local mode
        alivePlayers = new bool[localPlayerCount];
        for (int i = 0; i < alivePlayers.Length; i++)
        {
            alivePlayers[i] = true;
        }

        Debug.Log($"Initialized in local mode with {localPlayerCount} players. LocalLives: {string.Join(", ", localLives)}");
    }

    // Method to be called when a player kills another
    public void PlayerKilled(int playerID)
    {
        Debug.Log($"PlayerKilled called for player {playerID}");

        if (NetworkManager && NetworkManager.IsListening)
        {
            if (IsServer)
            {
                // The server can update directly
                DecreaseLives(playerID);
            }
            else
            {
                // Clients send an RPC to the server
                SubmitKillServerRpc(playerID);
            }
        }
        else
        {
            // Local mode - check array bounds
            if (playerID >= 0 && playerID < localLives.Length)
            {
                DecreaseLives(playerID);
            }
            else
            {
                Debug.LogError($"Invalid player ID in local mode: {playerID}. Valid range: 0-{localLives.Length - 1}");
            }
        }
    }

    // Decrease a life from the player who died
    private void DecreaseLives(int playerID)
    {
        Debug.Log($"DecreaseLives for player {playerID}");

        // Check if the player ID is valid
        if (playerID < 0 || (NetworkManager && playerID >= playerLives.Length) || (!NetworkManager && playerID >= localLives.Length))
        {
            Debug.LogError($"Invalid player ID: {playerID}");
            return;
        }

        // Decrement player lives
        if (NetworkManager && NetworkManager.IsListening)
        {
            if (IsServer)
            {
                playerLives[playerID]--;
                Debug.Log($"Remaining lives for player {playerID}: {playerLives[playerID]}");

                // Notify all clients to update the UI with the new lives values
                SyncLivesToClientRpc(playerLives);
            }
        }
        else
        {
            // Check array bounds in local mode
            if (playerID < localLives.Length)
            {
                localLives[playerID]--;
                Debug.Log($"Remaining lives for player {playerID} in local mode: {localLives[playerID]}");

                // In local mode, we update the UI directly
                UpdateUI();
            }
        }

        // Check if the player is out of lives
        if (GetLives(playerID) <= 0)
        {
            if (playerID < alivePlayers.Length)
            {
                alivePlayers[playerID] = false;

                int aliveCount = 0;
                for (int i = 0; i < alivePlayers.Length; i++)
                {
                    if (alivePlayers[i]) aliveCount++;
                }

                if (aliveCount == 1 && !gameEnded)
                {
                    gameEnded = true;
                    int winnerID = -1;
                    for (int i = 0; i < alivePlayers.Length; i++)
                    {
                        if (alivePlayers[i])
                        {
                            winnerID = i;
                            break;
                        }
                    }

                    if (winnerID >= 0)
                    {
                        if (NetworkManager && NetworkManager.IsListening && IsServer)
                        {
                            // In network mode, only the server determines the winner
                            AnnounceWinnerClientRpc(winnerID);
                        }
                        else
                        {
                            // In local mode, determine the winner directly
                            DetermineWinner(winnerID);
                        }
                    }
                }
            }
        }
    }

    // Method to get lives of a player (based on their ID)
    private int GetLives(int playerID)
    {
        if (NetworkManager && NetworkManager.IsListening)
        {
            return (playerID >= 0 && playerID < playerLives.Length) ? playerLives[playerID] : 0;
        }
        else
        {
            return (playerID >= 0 && playerID < localLives.Length) ? localLives[playerID] : 0;
        }
    }

    // Associate existing TextMeshPro and activate/deactivate based on active players
    private void GeneratePlayerTextMeshes()
    {
        playerTextMeshes = new TextMeshProUGUI[] { P1TextMesh, P2TextMesh, P3TextMesh, P4TextMesh };

        // Number of players in multiplayer or local mode
        int numberOfPlayers = 0;

        if (NetworkManager && NetworkManager.IsListening)
        {
            // In network mode, we use the number of connected clients
            numberOfPlayers = NetworkManager.ConnectedClientsIds.Count;
            Debug.Log($"Network mode: {numberOfPlayers} connected clients");
        }
        else
        {
            numberOfPlayers = PlayerConfigurationManager.Instance.readyCount;

            // If we don't find players, we try with PlayerConfigurationManager
            if (numberOfPlayers == 0 && PlayerConfigurationManager.Instance != null)
            {
                numberOfPlayers = PlayerConfigurationManager.Instance.readyCount;
                Debug.Log($"Using PlayerConfigurationManager to count players: {numberOfPlayers}");
            }

            // Make sure there is at least one player
            if (numberOfPlayers == 0)
            {
                numberOfPlayers = 1;
                Debug.LogWarning("No players detected, using 1 as default value");
            }

            // Update localPlayerCount variable
            localPlayerCount = numberOfPlayers;
        }

        Debug.Log($"Generating UI for {numberOfPlayers} players");

        // Activate/deactivate TextMeshPro based on number of players
        for (int i = 0; i < playerTextMeshes.Length; i++)
        {
            if (playerTextMeshes[i] != null)
            {
                bool shouldBeActive = i < numberOfPlayers;
                playerTextMeshes[i].gameObject.SetActive(shouldBeActive);

                if (shouldBeActive)
                {
                    Debug.Log($"Activating UI for player {i + 1}");
                }
                else
                {
                    Debug.Log($"Deactivating UI for player {i + 1}");
                }
            }
            else
            {
                Debug.LogError($"TextMeshPro at index {i} is null");
            }
        }
    }

    // Update UI based on player lives
    private void UpdateUI()
    {
        int playerCount = NetworkManager && NetworkManager.IsListening ?
            NetworkManager.ConnectedClientsIds.Count : localPlayerCount;

        for (int i = 0; i < playerTextMeshes.Length; i++)
        {
            if (playerTextMeshes[i] != null)
            {
                if (i < playerCount)
                {
                    int lives = GetLives(i);
                    playerTextMeshes[i].text = "P" + (i + 1) + ": " + lives + " Vidas";
                    playerTextMeshes[i].gameObject.SetActive(true);
                    Debug.Log($"Updated UI for player {i + 1}: {lives} lives");
                }
                else
                {
                    playerTextMeshes[i].gameObject.SetActive(false);
                }
            }
        }
    }

    // Determine the winner in local mode
    private void DetermineWinner(int winnerID)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player").Where(x => x.GetComponent<Player>()).ToArray();

        // Check if winnerID is within range of available players
        if (winnerID < players.Length && players[winnerID] != null)
        {
            if (PauseManager.instance != null)
            {
                PauseManager.instance.EndGame(players[winnerID].GetComponent<Player>().CharacterData);
            }
            else
            {
                Debug.LogError("PauseManager.instance is null, cannot end the game");
            }
        }
    }

    #region RPCs
    // RPC for the server to receive kill notifications
    [ServerRpc(RequireOwnership = false)]
    private void SubmitKillServerRpc(int playerID)
    {
        Debug.Log($"SubmitKillServerRpc received for player {playerID}");
        DecreaseLives(playerID);
    }

    // RPC to sync lives data from server to all clients
    [ClientRpc]
    private void SyncLivesToClientRpc(int[] livesData)
    {
        Debug.Log("SyncLivesToClientsRpc received");

        // Only update if we're not the server (server already has the latest data)
        if (!IsServer)
        {
            playerLives = livesData;

            // Update alivePlayers array based on lives
            for (int i = 0; i < playerLives.Length; i++)
            {
                alivePlayers[i] = playerLives[i] > 0;
            }
        }

        // Update UI with the new data
        UpdateUI();
    }

    // RPC to announce the winner to all clients
    [ClientRpc]
    private void AnnounceWinnerClientRpc(int winnerID)
    {
        Debug.Log($"AnnounceWinnerRpc received with winner ID: {winnerID}");

        // We only want to process this once
        if (!gameEnded)
        {
            gameEnded = true;

            // In networked mode, determine the winner on all clients
            DetermineWinner(winnerID);
        }
    }
    #endregion
}