using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private string sceneName;
    private LateJoinsBehaviour lateJoinsBehaviour;
    private List<Transform> spawnPoints;
    [SerializeField] private float respawnDelay = 1f;
    // Singleton Pattern
    // --------------------------------------------------------------------------------

    public static PlayerSpawner Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("[Singleton] Trying to instantiate a seccond instance of a singleton class.");
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }

        spawnPoints = new List<Transform>();
        foreach (Transform child in transform)
        {
            spawnPoints.Add(child);
        }

        if(!NetworkManager)
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
        GameObject[] players = tagPlayer.Where(x => {
                                            Player player = x.GetComponent<Player>();
                                            return player != null && player.isReady;
                                        }).ToArray();

        for(int i = 0; players.Length > i; i++)
        {
            Transform spawnPoint = spawnPoints[i];
            players[i].transform.position = spawnPoint.position;
            players[i].transform.rotation = spawnPoint.rotation;
            if(i%2 == 0) players[i].GetComponent<Player>().isFacingRight = false;
            enablePlayerFields(players[i]);
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
    }
    public void RespawnPlayer(GameObject player)
    {
        //if (!IsServer) return; // Solo el servidor maneja el respawn
        // Desactivar temporalmente el jugador 
        //player.GetComponent<Player>().enabled = false;
        //player.GetComponent<Collider2D>().enabled = false;
        //player.GetComponent<SpriteRenderer>().enabled = false;

        // Esperar un tiempo antes del respawn 
        StartCoroutine(RespawnAfterDelay(player, respawnDelay));
    }

    private IEnumerator RespawnAfterDelay(GameObject player, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Seleccionar punto de spawn aleatorio
        Transform randomSpawn = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];

        // Mover el jugador vidor)
        player.transform.position = randomSpawn.position;
        player.transform.rotation = randomSpawn.rotation;

        // Reactivar componentes
        player.SetActive(true);
        //player.GetComponent<Player>().enabled = true;
        //player.GetComponent<Collider2D>().enabled = true;
        //player.GetComponent<SpriteRenderer>().enabled = true;

        // Sincronizar posicion con clientes (Net)
        if (IsServer)
        {
            if (player.TryGetComponent<NetworkObject>(out var netObj))
            {
                UpdatePlayerPositionClientRpc(netObj, randomSpawn.position, randomSpawn.rotation);
            }
        }// Solo el servidor maneja el respawn

    }

    [ClientRpc]
    private void UpdatePlayerPositionClientRpc(NetworkObjectReference playerRef, Vector3 position, Quaternion rotation)
    {
        if (playerRef.TryGet(out NetworkObject netObj))
        {
            netObj.transform.position = position;
            netObj.transform.rotation = rotation;
        }
    }
    void OnDisable()
    {
        if(!NetworkManager)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoaded;
            NetworkManager.Singleton.SceneManager.OnUnload += UnSceceLoaded;
            lateJoinsBehaviour = FindObjectOfType<LateJoinsBehaviour>();
        }

    }

    private void UnSceceLoaded(ulong clientId, string sceneName, AsyncOperation asyncOperation)
    {
        if (IsServer && sceneName == sceneName)
        {
            //lateJoinsBehaviour.aprovedConection = true;
            foreach (ulong id in NetworkManager.ConnectedClientsIds)
            {
                if (id != OwnerClientId)
                {
                    NetworkObject playerNetworkObject = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.transform.GetChild(0).GetComponent<NetworkObject>();
                    playerNetworkObject.Despawn(true);
                }

            }
        }
    }

    private void SceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (IsServer && sceneName == sceneName)
        {
            Debug.Log("SceneLoaded");   
            LateJoinsBehaviour.aprovedConection = false;
            foreach (ulong id in clientsCompleted)
            {
                //if (id != OwnerClientId)
                //{
                    Debug.Log("PlayerSpawner: " + id);
                    //Pongos los fighters como hijos del player
                    //arrayPlayers[id].GetComponent<PlayerNetworkConfig>().InstantiateCharacterServerRpc(id);
                    GameObject playerGameObject = Instantiate(playerPrefab);
                    playerGameObject.GetComponent<NetworkObject>().SpawnWithOwnership(id);
                    playerGameObject.transform.SetParent(NetworkManager.Singleton.ConnectedClients[id].PlayerObject.transform, false);
                    playerGameObject.transform.parent.position = Vector2.zero;
                playerGameObject.transform.localPosition = Vector2.zero;
                //DesactivateMovementClientRPC(playerGameObject.GetComponent<NetworkObject>());
                    //NetworkManager.Singleton.ConnectedClients[id].PlayerObject;
                    //PlayerNetworkConfig.Instance.InstantiateCharacterServerRpc(id);
                    //player.transform.SetParent(transform, false);
                //}

            }
        }
    }
    [ClientRpc]
    private void DesactivateMovementClientRPC(NetworkObjectReference playerNetworkObjectReference)
    {
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        //PlayerController playerController = playerNetworkObject.GetComponent<PlayerController>();

        //playerController.enabled = false;
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsServer) return;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneLoaded;
        NetworkManager.Singleton.SceneManager.OnUnload -= UnSceceLoaded;

    }
}
