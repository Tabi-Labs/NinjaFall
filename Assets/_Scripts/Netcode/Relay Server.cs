using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RelayServer : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI code;
    [SerializeField]
    private TextMeshProUGUI showCode;
    [SerializeField]
    private string targetScene;
    public static string staticCode;
    //private LateJoinsBehaviour lateJoinsBehaviour;
    //Funci�n as�ncrona
    private async void Start()
    {
        //Inicializar los servicios de Unity
        //Es un a funci�n as�ncrona por lo tanto el m�todo tiene que ser asinc
        //Evita que se congele para los dem�s usuarios
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        //Se inicia la sesi�n de manera an�nima
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        //lateJoinsBehaviour = FindFirstObjectByType<LateJoinsBehaviour>();
        //if (lateJoinsBehaviour != null)
        //{
        //    Destroy(lateJoinsBehaviour.gameObject);
        //}

    }

    public async void CreateRelay()
    {
        try
        {
            //El argumento es el n�mero m�ximo de jugadores sin contar el host
            //Creamos una "conexi�n" con un c�digo
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(24);

            //C�digo para iniciar la partida
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            //RelayServerData relayServerData = new RelayServerData(allocation,"dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
                );
            NetworkManager.Singleton.StartHost();
            staticCode = joinCode;
            NetworkManager.Singleton.SceneManager.LoadScene(targetScene, LoadSceneMode.Single);
            //this.gameObject.SetActive(false);
            Debug.Log(joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.Log("Error: " + e);
        }

    }
    public async void JoinRelay(string joinCode)
    {
        //Asignamos el texto que se ha introducido
        joinCode = code.text;
        joinCode = joinCode.ToUpper();
        staticCode = joinCode;
        try
        {
            //Problema que surge con el textMeshPro
            joinCode = joinCode.Substring(0, 6);
            Debug.Log("Joining Relay with " + joinCode);
            //Nos unimos mediante el c�digo
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            //Manejamos el Relay a trav�s de nuestro Unity Transport
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                );

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log("Error: " + e);
        }
    }
}
