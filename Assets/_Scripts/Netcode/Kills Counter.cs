using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

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
    private int[] localLives = new int[4] { 5, 5, 5, 5 };

    // Variable para el modo local, la cantidad de jugadores locales (por ejemplo, 1, 2, 3 o 4) tamaño del array de players
    private int localPlayerCount = 3;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Inicializamos las NetworkVariables para las vidas de cada jugador
        for (int i = 0; i < 4; i++)
        {
            playerLives[i] = new NetworkVariable<int>(5);
        }
        Initialize();
    }
    private void Awake()
    {
        if (!NetworkManager)
            Initialize();
    }
    private void Initialize()
    {
        Instance = this;
        GeneratePlayerTextMeshes();  // Generamos los TextMeshPro en la UI
        UpdateUI();  // Actualizamos la UI según las vidas iniciales de los jugadores
    }

    // Método para ser llamado cuando un jugador mata a otro
    public void PlayerKilled(int playerID)
    {

            SubmitKillServerRpc(playerID);

    }

    // Restamos una vida al jugador que murió
    private void DecreaseLives(int playerID)
    {
        Debug.Log($"Jugador {playerID + 1} ha sido eliminado. Vidas restantes: {GetLives(playerID) - 1}");
        // Decrementamos las vidas del jugador
        playerLives[playerID].Value--;

        // Comprobamos si el jugador se quedó sin vidas
        if (GetLives(playerID) <= 0)
        {
            // Aquí podrías llamar a un método que reinicie al jugador o termine la partida.
            // Por ejemplo, podrías reiniciar al jugador o marcarlo como eliminado.
            PlayerEliminated(playerID);
        }

        // Actualizamos la UI
        UpdateUI();
    }

    // Método para obtener las vidas de un jugador (basado en su ID)
    private int GetLives(int playerID)
    {
        return (NetworkManager) ? playerLives[playerID].Value : localLives[playerID];
    }

    // Método que maneja lo que pasa cuando un jugador se queda sin vidas
    private void PlayerEliminated(int playerID)
    {
        // Aquí podrías reiniciar al jugador o terminar el juego, dependiendo de tu diseño de juego.
        // Por ejemplo, ocultar al jugador, ponerlo en un estado de espectador, o terminar el juego si todos los jugadores se han eliminado.
        Debug.Log($"Jugador {playerID + 1} eliminado. ¡Juego terminado!");
    }

    // Asocia los TextMeshPro existentes y activa/desactiva según los jugadores activos
    private void GeneratePlayerTextMeshes()
    {
        playerTextMeshes = new TextMeshProUGUI[] { P1TextMesh, P2TextMesh, P3TextMesh, P4TextMesh };

        // Si estamos en modo multijugador, usamos el número de clientes conectados
        int numberOfPlayers = (IsServer || IsHost) ?
            NetworkManager.Singleton.ConnectedClients.Count : localPlayerCount;

        // Aquí activamos/desactivamos los TextMeshPro según el número de jugadores
        for (int i = 0; i < playerTextMeshes.Length; i++)
        {
            if (i < 3)
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
