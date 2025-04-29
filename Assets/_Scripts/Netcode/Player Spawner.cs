using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private string sceneName;
    private LateJoinsBehaviour lateJoinsBehaviour;
    private List<Transform> spawnPoints;
    [SerializeField] private float respawnDelay = 1f;

    // Singleton Pattern
    public static PlayerSpawner Instance { get; private set; }

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

        // Inicializar puntos de spawn
        spawnPoints = new List<Transform>();
        foreach (Transform child in transform)
        {
            spawnPoints.Add(child);
        }

        // Suscribirse al evento de carga de escenas
        if (!NetworkManager)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SpawnPlayers();
    }

    public void SpawnPlayers()
    {
        GameObject[] tagPlayer = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] players = tagPlayer.Where(x =>
        {
            Player player = x.GetComponent<Player>();
            return player != null;
        }).ToArray();

        for (int i = 0; i < players.Length; i++)
        {
            Transform spawnPoint = spawnPoints[i];
            players[i].transform.position = spawnPoint.position;
            players[i].transform.rotation = spawnPoint.rotation;

            if (i % 2 == 0)
            {
                players[i].GetComponent<Player>().isFacingRight = false;
            }

            // Habilitar campos del jugador
            enablePlayerFields(players[i]);

            // Sincronizar con los clientes si estamos en red
            if (NetworkManager)
            {
                if (players[i].TryGetComponent<NetworkObject>(out var netObj))
                {
                    Debug.Log("Spawn");
                    UpdatePlayerPositionClientRpc(netObj, spawnPoint.position, spawnPoint.rotation);
                    EnablePlayerFieldsClientRpc(netObj);
                }
            }
        }
    }

    private void enablePlayerFields(GameObject player)
    {
        MonoBehaviour[] components = player.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour component in components)
        {
            component.enabled = true;
        }
        player.GetComponent<SpriteRenderer>().enabled = true;
        player.GetComponent<Animator>().enabled = true;
        player.GetComponent<Player>().FeetColl.enabled = true;
        player.GetComponent<Player>().HeadColl.enabled = true;
        player.GetComponent<Player>().BodyColl.enabled = true;
    }

    public void RespawnPlayer(GameObject player, int playerID)
    {
        if (!KillsCounter.Instance.alivePlayers[playerID]) return;
        StartCoroutine(RespawnAfterDelay(player, respawnDelay));
    }

    private IEnumerator RespawnAfterDelay(GameObject player, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Seleccionar punto de spawn aleatorio
        Transform randomSpawn = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];

        // Mover el jugador
        player.transform.position = randomSpawn.position;
        player.transform.rotation = randomSpawn.rotation;

        enablePlayerFields(player);
        player.GetComponent<Player>().IsDead = false;

        // Sincronizar posición y habilitación de campos con clientes (Net)
        if (IsServer)
        {
            if (player.TryGetComponent<NetworkObject>(out var netObj))
            {
                UpdatePlayerPositionClientRpc(netObj, randomSpawn.position, randomSpawn.rotation);
                EnablePlayerFieldsClientRpc(netObj);
            }
        }
    }

    [ClientRpc]
    private void UpdatePlayerPositionClientRpc(NetworkObjectReference playerRef, Vector3 position, Quaternion rotation)
    {
        if (playerRef.TryGet(out NetworkObject netObj))
        {
            netObj.transform.position = position;
            netObj.transform.rotation = rotation;
        }
        Debug.Log("Actualizado: " + playerRef.NetworkObjectId);
    }

    [ClientRpc]
    private void EnablePlayerFieldsClientRpc(NetworkObjectReference playerRef)
    {
        if (playerRef.TryGet(out NetworkObject netObj))
        {
            GameObject player = netObj.gameObject;

            MonoBehaviour[] components = player.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in components)
            {
                component.enabled = true;
            }
            player.GetComponent<SpriteRenderer>().enabled = true;
            player.GetComponent<Animator>().enabled = true;
            player.GetComponent<Player>().FeetColl.enabled = true;
            player.GetComponent<Player>().HeadColl.enabled = true;
            player.GetComponent<Player>().BodyColl.enabled = true;

            Debug.Log("Campos habilitados para el jugador: " + player.name);
        }
    }

    private void OnDisable()
    {
        if (!NetworkManager)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            Debug.Log("OnNetworkSpawn: Spawneando jugadores...");
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoaded;
            NetworkManager.Singleton.SceneManager.OnUnload += UnSceceLoaded;
            lateJoinsBehaviour = FindObjectOfType<LateJoinsBehaviour>();
        }
    }

    private void UnSceceLoaded(ulong clientId, string sceneName, AsyncOperation asyncOperation)
    {
        foreach (ulong id in NetworkManager.ConnectedClientsIds)
        {
            if (id != OwnerClientId)
            {
                NetworkObject playerNetworkObject = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.transform.GetChild(0).GetComponent<NetworkObject>();
                playerNetworkObject.Despawn(true);
            }
        }
    }

    private void SceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        SpawnPlayers();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsServer) return;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneLoaded;
        NetworkManager.Singleton.SceneManager.OnUnload -= UnSceceLoaded;
    }
}
