using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

[InitializeOnLoad]
public class BotManager : MonoBehaviour
{
    public static BotManager Instance { get; private set; }

    public List<BotController> bots = new List<BotController>();
    public GameObject playerPrefab; // Prefab del jugador
    public InputActionAsset playerActions; // Las acciones input comunes

    static BotManager()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

#if UNITY_EDITOR
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Debug.Log("Exiting Play Mode: cleanup here.");
            if (Instance != null)
            {
                Instance.CleanupBots();
            }
        }
    }
#endif

    void Update()
    {
        // Cada x frames, puedes hacer algo con los bots
        // Por ejemplo, hacer que salten
        if (Time.frameCount % 180 == 0) // Cada 60 frames
        {
            foreach (var bot in bots)
            {
                _ = bot.PressAndReleaseButton(GamepadButton.South, 0.5f); // Botón A
            }
        }
    }

    public BotController CreateBot()
    {
        PlayerInput pi = PlayerInputManager.instance.JoinPlayer(controlScheme: "Gamepad", pairWithDevice: InputSystem.AddDevice<Gamepad>());
        Gamepad virtualGamepad = pi.GetDevice<Gamepad>();
        BotController botController = new BotController(pi, virtualGamepad);
        bots.Add(botController);
        return botController;
    }

    public void CreateBot2(){
        PlayerInput pi = PlayerInputManager.instance.JoinPlayer(controlScheme: "Gamepad", pairWithDevice: InputSystem.AddDevice<Gamepad>());
        Gamepad virtualGamepad = pi.GetDevice<Gamepad>();
        BotController botController = new BotController(pi, virtualGamepad);
        bots.Add(botController);
    }

    public void CleanupBots()
    {
        foreach (var bot in bots)
        {
            InputSystem.RemoveDevice(bot.virtualGamepad);
        }
        bots.Clear();
    }
}
