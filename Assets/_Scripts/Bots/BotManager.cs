using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class BotManager : MonoBehaviour
{
    public static BotManager Instance { get; private set; }

    public Dictionary<int, BotController> bots = new Dictionary<int, BotController>();
    public GameObject playerPrefab; // Prefab del jugador
    public InputActionAsset playerActions; // Las acciones input comunes
    private int botId = 0;

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

    // Callbacks
    // --------------------------------------------------------------------------------

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

    private void OnDestroy()
    {
        CleanupBots();
    }

    // Bot Management
    // --------------------------------------------------------------------------------

    public int CreateBot()
    {
        PlayerInput pi = PlayerInputManager.instance.JoinPlayer(controlScheme: "Gamepad", pairWithDevice: InputSystem.AddDevice<Gamepad>());
        Gamepad virtualGamepad = pi.GetDevice<Gamepad>();
        BotController botController = new BotController(pi, virtualGamepad);
        bots.Add(++botId, botController);
        BotBrain botBrain = pi.AddComponent<BotBrain>();
        botBrain.botId = botId;
        botBrain.enabled = false;

        return botId;
    }

    public BotController GetBotById(int botId)
    {
        if (bots.TryGetValue(botId, out var bot))
        {
            return bot;
        }
        return null;
    }

    public void CleanupBots()
    {
        foreach (BotController bot in bots.Values)
        {
            Gamepad currentGamepad = bot.virtualGamepad;
            Destroy(bot.playerInput.gameObject);
            InputSystem.RemoveDevice(currentGamepad);
        }
        bots.Clear();
    }

    public void StopBots(){
        foreach (BotController bot in bots.Values)
        {
            bot.ResetInputs();
        }
    }

    public void RemoveBotById(int botId)
    {
        if (bots.ContainsKey(botId))
        {
            InputSystem.RemoveDevice(bots[botId].virtualGamepad);
            bots.Remove(botId);
        }
    }
}
