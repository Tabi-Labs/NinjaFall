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
    // Arreglo para las vidas de cada jugador
    public NetworkVariable<int>[] playerLives;

    // Referencias a los TextMeshProUGUI ya existentes en la escena
    [SerializeField] private TextMeshProUGUI P1TextMesh;
    [SerializeField] private TextMeshProUGUI P2TextMesh;
    [SerializeField] private TextMeshProUGUI P3TextMesh;
    [SerializeField] private TextMeshProUGUI P4TextMesh;

    private TextMeshProUGUI[] playerTextMeshes;
    private int[] localLives = new int[4] { MAX_LIVES, MAX_LIVES, MAX_LIVES, MAX_LIVES };
    public bool[] alivePlayers;

    // Variable para el modo local, la cantidad de jugadores locales
    private int localPlayerCount;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Inicializar para modo local o red
        if (!NetworkManager)
        {
            // Usar FindObjectsOfType para determinar el número de jugadores activos
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            localPlayerCount = players.Length;

            // Si no hay jugadores detectados, intentar desde PlayerConfigurationManager
            if (localPlayerCount == 0 && PlayerConfigurationManager.Instance != null)
            {
                localPlayerCount = PlayerConfigurationManager.Instance.readyCount;
            }

            // Asegurar que tenemos al menos un jugador para evitar errores
            if (localPlayerCount == 0)
            {
                localPlayerCount = 1;
                Debug.LogWarning("No se detectaron jugadores, estableciendo localPlayerCount a 1");
            }

            Debug.Log($"KillsCounter: Inicializando en modo local con {localPlayerCount} jugadores");
            InitializeLocalMode();

            // Generar los TextMeshPro en la UI y actualizar UI
            GeneratePlayerTextMeshes();
            UpdateUI();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Inicializar las NetworkVariables
        if (IsServer)
        {
            Debug.Log("Inicializando KillsCounter en el servidor");
            InitializeNetworkMode();
        }
        else
        {
            Debug.Log("Inicializando KillsCounter en el cliente");
            int playerCount = NetworkManager.ConnectedClientsIds.Count;
            playerLives = new NetworkVariable<int>[playerCount];

            for (int i = 0; i < playerCount; i++)
            {
                // En el cliente, inicializamos las NetworkVariable con valor predeterminado 0
                // El servidor sincronizará los valores reales
                playerLives[i] = new NetworkVariable<int>(MAX_LIVES, NetworkVariableReadPermission.Everyone);
            }

            // Inicializar alivePlayers para el cliente
            alivePlayers = new bool[playerCount];
            for (int i = 0; i < alivePlayers.Length; i++)
            {
                alivePlayers[i] = true;
            }
        }

        // Generar los TextMeshPro en la UI
        GeneratePlayerTextMeshes();

        // Actualizar la UI según las vidas iniciales de los jugadores
        UpdateUI();
    }

    private void InitializeNetworkMode()
    {
        // Crear las NetworkVariables
        int playerCount = NetworkManager.ConnectedClientsIds.Count;
        playerLives = new NetworkVariable<int>[playerCount];

        for (int i = 0; i < playerCount; i++)
        {
            playerLives[i] = new NetworkVariable<int>(MAX_LIVES, NetworkVariableReadPermission.Everyone);
        }

        // Inicializar alivePlayers
        alivePlayers = new bool[playerCount];
        for (int i = 0; i < alivePlayers.Length; i++)
        {
            alivePlayers[i] = true;
        }

        Debug.Log($"Inicializado en modo red con {playerCount} jugadores");
    }

    private void InitializeLocalMode()
    {
        // Inicializar arreglo de vidas locales
        localLives = new int[localPlayerCount];
        for (int i = 0; i < localPlayerCount; i++)
        {
            localLives[i] = MAX_LIVES;
        }

        // Inicializar alivePlayers para modo local
        alivePlayers = new bool[localPlayerCount];
        for (int i = 0; i < alivePlayers.Length; i++)
        {
            alivePlayers[i] = true;
        }

        Debug.Log($"Inicializado en modo local con {localPlayerCount} jugadores. LocalLives: {string.Join(", ", localLives)}");
    }

    // Método para ser llamado cuando un jugador mata a otro
    public void PlayerKilled(int playerID)
    {
        Debug.Log($"PlayerKilled llamado para jugador {playerID}");

        if (NetworkManager && NetworkManager.IsListening)
        {
            if (IsServer)
            {
                // El servidor puede actualizar directamente
                DecreaseLives(playerID);
            }
            else
            {
                // Los clientes envían un RPC al servidor
                SubmitKillServerRpc(playerID);
            }
        }
        else
        {
            // Modo local - verificar límites del array
            if (playerID >= 0 && playerID < localLives.Length)
            {
                DecreaseLives(playerID);
            }
            else
            {
                Debug.LogError($"ID de jugador inválido en modo local: {playerID}. Rango válido: 0-{localLives.Length - 1}");
            }
        }
    }

    // Restamos una vida al jugador que murió
    private void DecreaseLives(int playerID)
    {
        Debug.Log($"DecreaseLives para jugador {playerID}");

        // Verificar que el ID del jugador es válido
        if (playerID < 0 || (NetworkManager && playerID >= playerLives.Length) || (!NetworkManager && playerID >= localLives.Length))
        {
            Debug.LogError($"ID de jugador inválido: {playerID}");
            return;
        }

        // Decrementar las vidas del jugador
        if (NetworkManager && NetworkManager.IsListening)
        {
            if (IsServer && playerLives[playerID] != null)
            {
                playerLives[playerID].Value--;
                Debug.Log($"Vidas restantes para jugador {playerID}: {playerLives[playerID].Value}");

                // Notificar a todos los clientes para actualizar la UI
                UpdateUIClientRpc();
            }
        }
        else
        {
            // Verificar límites del array en modo local
            if (playerID < localLives.Length)
            {
                localLives[playerID]--;
                Debug.Log($"Vidas restantes para jugador {playerID} en modo local: {localLives[playerID]}");

                // En modo local, actualizamos la UI directamente
                UpdateUI();
            }
        }

        // Comprobar si el jugador se quedó sin vidas
        if (GetLives(playerID) <= 0)
        {
            if (playerID < alivePlayers.Length)
            {
                alivePlayers[playerID] = false;

                int aliveCount = 0;
                for (int i = 0; i < alivePlayers.Length; i++)
                {
                    if (alivePlayers[i] == true) aliveCount++;
                }

                if (aliveCount == 1)
                {
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
                        GameObject[] players = GameObject.FindGameObjectsWithTag("Player").Where(x => x.GetComponent<Player>()).ToArray();

                        // Verificar que el winnerID está dentro del rango de jugadores disponibles
                        if (winnerID < players.Length && players[winnerID] != null)
                        {
                            if (PauseManager.instance != null)
                            {
                                PauseManager.instance.EndGame(players[winnerID].GetComponent<Player>().CharacterData);
                            }
                            else
                            {
                                Debug.LogError("PauseManager.instance es null, no se puede finalizar el juego");
                            }
                        }
                    }
                }
            }
        }
    }

    // Método para obtener las vidas de un jugador (basado en su ID)
    private int GetLives(int playerID)
    {
        if (NetworkManager && NetworkManager.IsListening)
        {
            return (playerID >= 0 && playerID < playerLives.Length && playerLives[playerID] != null) ?
                playerLives[playerID].Value : 0;
        }
        else
        {
            return (playerID >= 0 && playerID < localLives.Length) ? localLives[playerID] : 0;
        }
    }

    // Asocia los TextMeshPro existentes y activa/desactiva según los jugadores activos
    private void GeneratePlayerTextMeshes()
    {
        playerTextMeshes = new TextMeshProUGUI[] { P1TextMesh, P2TextMesh, P3TextMesh, P4TextMesh };

        // Número de jugadores en modo multijugador o local
        int numberOfPlayers = NetworkManager && NetworkManager.IsListening ?
            NetworkManager.ConnectedClientsIds.Count : localPlayerCount;

        // Activar/desactivar los TextMeshPro según el número de jugadores
        for (int i = 0; i < playerTextMeshes.Length; i++)
        {
            if (playerTextMeshes[i] != null)
            {
                if (i < numberOfPlayers)
                {
                    playerTextMeshes[i].gameObject.SetActive(true);  // Activamos los TextMeshPro de jugadores activos
                }
                else
                {
                    playerTextMeshes[i].gameObject.SetActive(false); // Desactivamos los TextMeshPro de jugadores inactivos
                }
            }
        }

        Debug.Log($"TextMeshPro generados para {numberOfPlayers} jugadores");
    }

    // Actualizamos la UI en todos los clientes
    private void UpdateUI()
    {
        for (int i = 0; i < playerTextMeshes.Length; i++)
        {
            if (playerTextMeshes[i] != null && playerTextMeshes[i].gameObject.activeSelf)
            {
                int lives = GetLives(i);
                playerTextMeshes[i].text = "P" + (i + 1) + ": " + lives + " Vidas";
                Debug.Log($"Actualizado UI para jugador {i}: {lives} vidas");
            }
        }
    }

    // RPC para que el servidor actualice el contador de muertes y vidas
    [ServerRpc(RequireOwnership = false)]
    private void SubmitKillServerRpc(int playerID)
    {
        Debug.Log($"SubmitKillServerRpc recibido para jugador {playerID}");
        DecreaseLives(playerID);
    }

    // RPC para actualizar la UI en todos los clientes
    [ClientRpc]
    private void UpdateUIClientRpc()
    {
        Debug.Log("UpdateUIClientRpc recibido");
        UpdateUI();
    }
}
