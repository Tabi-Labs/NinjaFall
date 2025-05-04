using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LevelSelector : NetworkBehaviour
{
    public GameObject[] levelPrefabs;
    private NetworkVariable<int> idScene = new NetworkVariable<int>(-1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    void Start()
    {
        if (NetworkManager) return;
        RandomLevelSelector();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        idScene.OnValueChanged += OnIdSceneChanged;
        if (IsServer)
        {
            int randomIndex = Random.Range(0, levelPrefabs.Length);
            idScene.Value = randomIndex;
        }
    }

    private void OnIdSceneChanged(int previousValue, int newValue)
    {
        ApplyLevelSelection(newValue);
    }

    private void RandomLevelSelector()
    {
        // Choose a random level prefab from the list
        int randomIndex = Random.Range(0, levelPrefabs.Length);
        ApplyLevelSelection(randomIndex);
    }
    [Rpc(SendTo.Everyone)]
    private void RandomLevelSelectorClientRpc()
    {
        if (!IsServer)
        {
            ApplyLevelSelection(idScene.Value);
        }

    }
    private void ApplyLevelSelection(int index)
    {
        if(NetworkManager)
        {
            Debug.Log("ID: " + index);
            GameObject levelInstance = Instantiate(levelPrefabs[index], Vector3.zero, Quaternion.identity);
            NetworkObject networkObject = levelInstance.GetComponent<NetworkObject>();
            networkObject.Spawn();
        }
        else
        {
            Instantiate(levelPrefabs[index], Vector3.zero, Quaternion.identity);
        }
                         
    }
}
