using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Linq;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class KillsCounter : NetworkBehaviour
{
    public static KillsCounter Instance;
    private const int MAX_LIVES = 3;

    // Arreglo para almacenar las vidas de los jugadores (modo en red)
    private int[] playerLives;

    // Referencias a los TextMeshProUGUI en la escena
    [SerializeField] private TextMeshProUGUI P1TextMesh;
    [SerializeField] private TextMeshProUGUI P2TextMesh;
    [SerializeField] private TextMeshProUGUI P3TextMesh;
    [SerializeField] private TextMeshProUGUI P4TextMesh;

    private TextMeshProUGUI[] playerTextMeshes;
    private int[] localLives;
    public bool[] alivePlayers;

    private Player[] allPlayers;

    // Cantidad de jugadores locales
    private int localPlayerCount;

    // Bandera para saber si el juego ha terminado
    private bool gameEnded = false;

    private void Awake()
    {
        Instance = this;
        localLives = new int[4] {MAX_LIVES, MAX_LIVES, MAX_LIVES, MAX_LIVES};
    }

    private void Start()
    {
        // Inicialización para modo local
        if (!NetworkManager)
        {
            localPlayerCount = PlayerConfigurationManager.Instance.readyCount;

            if (localPlayerCount == 0 && PlayerConfigurationManager.Instance != null)
            {
                localPlayerCount = PlayerConfigurationManager.Instance.readyCount;
            }

            if (localPlayerCount == 0)
            {
                localPlayerCount = 1;
            }

            // Get all players in the scene
            allPlayers = GameObject.FindGameObjectsWithTag("Player")
                .Where(x => x.GetComponent<Player>() != null)
                .Select(x => x.GetComponent<Player>())
                .ToArray();

            allPlayers = allPlayers.OrderBy(x => x.GetComponent<PlayerInput>().playerIndex).ToArray();

            InitializeLocalMode();
            InitializeCounters();
            UpdateUI();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        int playerCount = NetworkManager.ConnectedClientsIds.Count;
        playerLives = new int[playerCount];

        for (int i = 0; i < playerCount; i++)
        {
            playerLives[i] = MAX_LIVES;
        }

        alivePlayers = new bool[playerCount];
        for (int i = 0; i < alivePlayers.Length; i++)
        {
            alivePlayers[i] = true;
        }

        // Get all players in the scene
        allPlayers = GameObject.FindGameObjectsWithTag("Player")
            .Where(x => x.GetComponent<Player>() != null)
            .Select(x => x.GetComponent<Player>())
            .ToArray();

        allPlayers = allPlayers.OrderBy(x => x.GetComponent<NetworkObject>().NetworkObjectId).ToArray();

        InitializeCounters();

        if (IsServer)
        {
            SyncLivesToClientRpc(playerLives);
        }

        UpdateUI();
    }

    private void InitializeLocalMode()
    {
        localLives = new int[localPlayerCount];
        for (int i = 0; i < localPlayerCount; i++)
        {
            localLives[i] = MAX_LIVES;
        }

        alivePlayers = new bool[localPlayerCount];
        for (int i = 0; i < alivePlayers.Length; i++)
        {
            alivePlayers[i] = true;
        }
    }

    // Método llamado cuando un jugador mata a otro
    public void PlayerKilled(int playerID)
    {
        if (NetworkManager && NetworkManager.IsListening)
        {
            if (IsServer)
            {
                DecreaseLives(playerID);
            }
            else
            {
                SubmitKillServerRpc(playerID);
            }
        }
        else
        {
            if (playerID >= 0 && playerID < localLives.Length)
            {
                DecreaseLives(playerID);
            }
        }
    }

    // Resta una vida al jugador especificado
    private void DecreaseLives(int playerID)
    {
        if (playerID < 0 || (NetworkManager && playerID >= playerLives.Length) || (!NetworkManager && playerID >= localLives.Length))
        {
            return;
        }

        if (NetworkManager && NetworkManager.IsListening)
        {
            if (IsServer)
            {
                playerLives[playerID]--;
                SyncLivesToClientRpc(playerLives);
            }
        }
        else
        {
            if (playerID < localLives.Length)
            {
                localLives[playerID]--;
                UpdateUI();
            }
        }

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
                        if (NetworkManager)
                        {
                            AnnounceWinnerServerRpc(winnerID);
                        }
                        else
                        {
                            DetermineWinner(winnerID);
                        }
                    }
                }
            }
        }
    }

    // Devuelve las vidas actuales de un jugador
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

    // Asocia los TextMeshPro existentes y los activa según los jugadores
    private void InitializeCounters()
    {
        playerTextMeshes = new TextMeshProUGUI[] { P1TextMesh, P2TextMesh, P3TextMesh, P4TextMesh };

        int numberOfPlayers = 0;

        if (NetworkManager && NetworkManager.IsListening)
        {
            numberOfPlayers = NetworkManager.ConnectedClientsIds.Count;
        }
        else
        {
            numberOfPlayers = PlayerConfigurationManager.Instance.readyCount;

            if (numberOfPlayers == 0 && PlayerConfigurationManager.Instance != null)
            {
                numberOfPlayers = PlayerConfigurationManager.Instance.readyCount;
            }

            if (numberOfPlayers == 0)
            {
                numberOfPlayers = 1;
            }

            localPlayerCount = numberOfPlayers;
        }

        for (int i = 0; i < numberOfPlayers; i++)
        {
            LivesCanvasHandler.Instance.InitializePanel(i, allPlayers[i].CharacterData, GetLives(i));
        }
    }

    // Actualiza la UI con las vidas de cada jugador
    private void UpdateUI()
    {
        int playerCount = NetworkManager && NetworkManager.IsListening ?
            NetworkManager.ConnectedClientsIds.Count : localPlayerCount;

        for (int i = 0; i < playerCount; i++){
            LivesCanvasHandler.Instance.UpdatePanels(i, GetLives(i));
        }

    }

    // Determina al ganador en modo local
    private void DetermineWinner(int winnerID)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player").Where(x => x.GetComponent<Player>()).ToArray();

        if (winnerID < players.Length && players[winnerID] != null)
        {
            if (PauseManager.instance != null)
            {
                UpdateUI();
                PauseManager.instance.EndGame(players[winnerID].GetComponent<Player>().CharacterData);
            }
        }

    }

    #region RPCs

    [ServerRpc(RequireOwnership = false)]
    private void SubmitKillServerRpc(int playerID)
    {
        DecreaseLives(playerID);
    }

    [ClientRpc]
    private void SyncLivesToClientRpc(int[] livesData)
    {
        if (!IsServer)
        {
            playerLives = livesData;

            for (int i = 0; i < playerLives.Length; i++)
            {
                alivePlayers[i] = playerLives[i] > 0;
            }
        }

        UpdateUI();
    }

    [Rpc(SendTo.Everyone)]
    private void AnnounceWinnerClientRpc(int winnerID)
    {

            gameEnded = true;
            DetermineWinner(winnerID);
    }
    [Rpc(SendTo.Server)]
    private void AnnounceWinnerServerRpc(int winnerID)
    {

        AnnounceWinnerClientRpc(winnerID);
        //DetermineWinner(winnerID);

    }

    #endregion
}
