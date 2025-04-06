using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Linq;

public class KillsCounter : NetworkBehaviour
{
    public static KillsCounter Instance;

    // Arreglo para las vidas de cada jugador, se inicializan en 5
    public NetworkVariable<int>[] playerLives = new NetworkVariable<int>[4];

    // Referencias a los TextMeshProUGUI ya existentes en la escena
    [SerializeField] private TextMeshProUGUI P1TextMesh;
    [SerializeField] private TextMeshProUGUI P2TextMesh;
    [SerializeField] private TextMeshProUGUI P3TextMesh;
    [SerializeField] private TextMeshProUGUI P4TextMesh;

    private TextMeshProUGUI[] playerTextMeshes;  // Arreglo de TextMeshProUGUI generados
    private int[] localLives = new int[4] { 1, 1, 1, 1 };
    public bool[] alivePlayers;

    // Variable para el modo local, la cantidad de jugadores locales (por ejemplo, 1, 2, 3 o 4) tama�o del array de players
    private int localPlayerCount;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Inicializamos las NetworkVariables para las vidas de cada jugador
        for (int i = 0; i < NetworkManager.ConnectedClients.Count; i++)
        {
            playerLives[i] = new NetworkVariable<int>(5);
        }
        Initialize();
    }
    private void Awake()
    {
        if (!NetworkManager)
        {
            localPlayerCount = PlayerConfigurationManager.Instance.readyCount;
            Initialize();
        }
        alivePlayers = new bool[localPlayerCount];
        for (int i = 0; i < alivePlayers.Length; i++)
        {
            alivePlayers[i] = true;
        }
    }
    private void Initialize()
    {
        Instance = this;
        GeneratePlayerTextMeshes();  // Generamos los TextMeshPro en la UI
        UpdateUI();  // Actualizamos la UI seg�n las vidas iniciales de los jugadores
    }

    // M�todo para ser llamado cuando un jugador mata a otro
    public void PlayerKilled(int playerID)
    {
            if(NetworkManager)
                SubmitKillServerRpc(playerID);
            else
                DecreaseLives(playerID);

    }

    // Restamos una vida al jugador que muri�
    private void DecreaseLives(int playerID)
    {
        Debug.Log($"Jugador {playerID + 1} ha sido eliminado. Vidas restantes: {GetLives(playerID) - 1}");
        // Decrementamos las vidas del jugador
        if (NetworkManager)
            playerLives[playerID].Value--;
        else
            localLives[playerID]--;


        // Comprobamos si el jugador se qued� sin vidas
        if (GetLives(playerID) <= 0)
        {
            alivePlayers[playerID] = false;

            int aliveCount = 0;
            for(int i = 0; i < alivePlayers.Length; i++)
            {
                if (alivePlayers[i] == true) aliveCount++;
            }
            if(aliveCount == 1)
            {
                int winnerID = alivePlayers.ToList().IndexOf(true);
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player").Where(x => x.GetComponent<Player>()).ToArray();
                PauseManager.instance.EndGame(players[winnerID].GetComponent<Player>().CharacterData);
            }
        }

        // Actualizamos la UI
        UpdateUI();
    }

    // M�todo para obtener las vidas de un jugador (basado en su ID)
    private int GetLives(int playerID)
    {
        return (NetworkManager) ? playerLives[playerID].Value : localLives[playerID];
    }

    // Asocia los TextMeshPro existentes y activa/desactiva seg�n los jugadores activos
    private void GeneratePlayerTextMeshes()
    {
        playerTextMeshes = new TextMeshProUGUI[] { P1TextMesh, P2TextMesh, P3TextMesh, P4TextMesh };

        // Si estamos en modo multijugador, usamos el n�mero de clientes conectados
        int numberOfPlayers = (IsServer || IsHost) ?
            NetworkManager.Singleton.ConnectedClients.Count : localPlayerCount;

        // Aqu� activamos/desactivamos los TextMeshPro seg�n el n�mero de jugadores
        for (int i = 0; i < playerTextMeshes.Length; i++)
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

    // Actualizamos la UI en todos los clientes
    private void UpdateUI()
    {
        for (int i = 0; i < playerTextMeshes.Length; i++)
        {
            if (playerTextMeshes[i] != null && playerTextMeshes[i].gameObject.activeSelf)
            {
                int lives = GetLives(i);
                playerTextMeshes[i].text = "P" + (i + 1) + ": " + lives + " Vidas";
            }
        }
    }

    // RPC para que el servidor actualice el contador de muertes y vidas
    [Rpc(SendTo.Everyone)]
    private void SubmitKillServerRpc(int playerID)
    {
        DecreaseLives(playerID);
    }
}
