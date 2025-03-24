using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class TestingNet : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button shutdownButton;
    [SerializeField] private Timer timer;

    private void Start()
    {
        if (hostButton) hostButton.onClick.AddListener(StartHost);
        if (clientButton) clientButton.onClick.AddListener(StartClient);
        if (shutdownButton) shutdownButton.onClick.AddListener(ShutdownNetwork);

        Debug.Log("Testing script initialized. Press H for Host, C for Client.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) StartHost();
        if (Input.GetKeyDown(KeyCode.C)) StartClient();
        if (Input.GetKeyDown(KeyCode.Escape)) ShutdownNetwork();
    }

    private void StartHost()
    {
        if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
        {
            Debug.Log("Starting Host...");
            NetworkManager.Singleton.StartHost();
            timer.CambiarVariable();
        }
        else
        {
            Debug.LogWarning("Already in a network session!");
        }
    }

    private void StartClient()
    {
        if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
        {
            Debug.Log("Starting Client...");
            NetworkManager.Singleton.StartClient();
        }
        else
        {
            Debug.LogWarning("Already in a network session!");
        }
    }

    private void ShutdownNetwork()
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
        {
            Debug.Log("Shutting down network...");
            NetworkManager.Singleton.Shutdown();
        }
        else
        {
            Debug.LogWarning("No active network session to shut down.");
        }
    }
}
