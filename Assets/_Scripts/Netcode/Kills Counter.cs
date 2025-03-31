using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class KillsCounter : NetworkBehaviour
{
    public static KillsCounter Instance;

    public int localDeathCount = 0; // Para modo local
    public NetworkVariable<int> deathCount = new NetworkVariable<int>(0); // Para multijugador
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayerDied()
    {
        if (IsServer || IsHost)
        {
            // Si es el servidor o estamos en modo local, sumamos directamente
            IncreaseDeathCount();
        }
        else
        {
            // Si es cliente en multijugador, pide al servidor que lo haga
            SubmitDeathServerRpc();
        }
    }

    private void IncreaseDeathCount()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
        {
            // En multijugador, actualizamos la NetworkVariable
            deathCount.Value++;
            textMeshProUGUI.text = "Kills: "+ deathCount.Value;
            Debug.Log("Death count: " + deathCount.Value);
        }
        else
        {
            // En modo local, usamos la variable local
            localDeathCount++;
            
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SubmitDeathServerRpc()
    {
        IncreaseDeathCount();
    }
}
