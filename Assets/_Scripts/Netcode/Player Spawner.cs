using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private string sceneName;
    private LateJoinsBehaviour lateJoinsBehaviour;


    //[SerializeField]
    //private Transform playerBucketTransform;


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
