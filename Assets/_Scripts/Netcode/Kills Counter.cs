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

    // Contenedor para los TextMeshProUGUI
    [SerializeField] private GameObject textPrefab;  // Prefab de TextMeshProUGUI
    [SerializeField] private Transform uiContainer;  // Contenedor donde se a�adir�n los textos

    private TextMeshProUGUI[] playerTextMeshes;  // Arreglo de TextMeshProUGUI generados

    private void Awake()
    {
        Instance = this;

        // Inicializamos las vidas de los jugadores
        playerLives[0] = new NetworkVariable<int>(5);
        playerLives[1] = new NetworkVariable<int>(5);
        playerLives[2] = new NetworkVariable<int>(5);
        playerLives[3] = new NetworkVariable<int>(5);
    }

    private void Start()
    {
        // Inicializamos la interfaz de usuario al principio, dependiendo de si estamos en el servidor o en cliente
        if (IsServer || IsHost)
        {
            GeneratePlayerTextMeshes();
            UpdateUI();
        }
    }

    // M�todo para ser llamado cuando un jugador mata a otro
    public void PlayerKilled(int playerID)
    {
        if (IsServer || IsHost)
        {
            // Restamos una vida al jugador que muri�
            DecreaseLives(playerID);
        }
        else
        {
            // Si es cliente, le pedimos al servidor que lo haga
            SubmitKillServerRpc(playerID);
        }
    }

    // Restamos una vida al jugador que muri�
    private void DecreaseLives(int playerID)
    {
        // Decrementamos las vidas del jugador
        playerLives[playerID].Value--;

        // Comprobamos si el jugador se qued� sin vidas
        if (GetLives(playerID) <= 0)
        {
            // Aqu� podr�as llamar a un m�todo que reinicie al jugador o termine la partida.
            // Por ejemplo, podr�as reiniciar al jugador o marcarlo como eliminado.
            PlayerEliminated(playerID);
        }

        // Actualizamos la UI
        UpdateUI();
    }

    // M�todo para obtener las vidas de un jugador (basado en su ID)
    private int GetLives(int playerID)
    {
        return playerLives[playerID].Value;
    }

    // M�todo que maneja lo que pasa cuando un jugador se queda sin vidas
    private void PlayerEliminated(int playerID)
    {
        // Aqu� podr�as reiniciar al jugador o terminar el juego, dependiendo de tu dise�o de juego.
        // Por ejemplo, ocultar al jugador, ponerlo en un estado de espectador, o terminar el juego si todos los jugadores se han eliminado.
        Debug.Log($"Jugador {playerID + 1} eliminado. �Juego terminado!");
    }

    // Genera los TextMeshProUGUI para cada jugador activo
    private void GeneratePlayerTextMeshes()
    {
        playerTextMeshes = new TextMeshProUGUI[4];  // Asumiendo que solo hay 4 jugadores en el juego

        // Creamos los TextMeshProUGUI para cada jugador (basado en el n�mero de jugadores activos)
        for (int i = 0; i < playerLives.Length; i++)
        {
            if (playerLives[i] != null)  // Solo generamos TextMeshProUGUI para jugadores activos
            {
                // Instanciamos el prefab de TextMeshPro
                TextMeshProUGUI playerTextMesh = Instantiate(textPrefab, uiContainer).GetComponent<TextMeshProUGUI>();
                playerTextMeshes[i] = playerTextMesh;
            }
        }
    }

    // Actualizamos la UI en todos los clientes
    private void UpdateUI()
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            // Iteramos a trav�s de los jugadores y actualizamos su UI
            for (int i = 0; i < playerLives.Length; i++)
            {
                // Aseguramos que solo actualizamos el texto para jugadores activos
                if (playerTextMeshes[i] != null)
                {
                    // Cambiamos el formato del texto a "P+IDJugador: X Vidas"
                    playerTextMeshes[i].text = "P" + (i + 1) + ": " + playerLives[i].Value + " Vidas";
                }
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
