using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class CharacterSelectorHandler : NetworkBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private float shakeAngle = 3.0f;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float scaleFactor = 0.06f;
    [SerializeField] private float luminosityFactor = 0.3f;
    [SerializeField] private string defaultTopText = "null";

    [Header("Audio Settings")]
    [SerializeField] private string selectionSoundID = "FX_ChooseCharacter";
    [SerializeField] private string changeSoundID = "FX_ChangeCharacter";
    [SerializeField] private string cancelSoundID = "FX_FreeCharacter";
    [SerializeField] private string denySoundID = "FX_Deny";

    // Referencias
    private RectTransform portraitPanel;
    private RectTransform emptyPanel;
    private Selectable nameButton;
    private Selectable leftArrow;
    private Selectable rightArrow;
    private MultiplayerEventSystem eventSystem;
    private Image portraitImage;
    private Outline portraitOutline;
    private Image nameImage;
    private Image overlay;
    private TextMeshProUGUI topText;
    private PlayerConfigurationManager pcm;

    // Propiedades
    public bool isAvailable { get; set; } = true; // Cambiado a público modificable
    public bool isSelected { get; set; } = false; // Cambiado a público modificable
    public int playerIndex { get; set; } = -1; // Cambiado a público modificable
    public PlayerInput pi { get; set; } = null; // Cambiado a público modificable
    public int CurrentCharacterIndex => currCharacterIdx;

    // Variables privadas
    private int currCharacterIdx = 0;
    private CharacterData currCharacterData;
    private bool canControl = false; // Solo se puede controlar si pertenece al jugador local

    // Propiedad auxiliar para verificar si estamos en modo red
    private bool IsNetworked => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    // Inicialización y configuración
    // --------------------------------------------------------------------------------

    void Start()
    {
        // Inicialización de referencias a componentes
        portraitPanel = transform.Find("PortraitPanel").GetComponent<RectTransform>();
        emptyPanel = transform.Find("EmptyPanel").GetComponent<RectTransform>();
        leftArrow = transform.Find("PortraitPanel/LeftArrow").GetComponent<Selectable>();
        rightArrow = transform.Find("PortraitPanel/RightArrow").GetComponent<Selectable>();
        eventSystem = transform.Find("EventSystem").GetComponent<MultiplayerEventSystem>();
        portraitImage = transform.Find("PortraitPanel/Portrait").GetComponent<Image>();
        overlay = transform.Find("PortraitPanel/Overlay").GetComponent<Image>();
        topText = transform.Find("TopText").GetComponent<TextMeshProUGUI>();

        portraitOutline = portraitImage.GetComponent<Outline>();

        Transform nameText = transform.Find("PortraitPanel/Name");
        nameButton = nameText.GetComponent<Selectable>();
        nameImage = nameText.GetComponent<Image>();

        pcm = PlayerConfigurationManager.Instance;

        topText.text = defaultTopText;

        // Agregar listeners para eventos de UI
        AddSelectionListeners(leftArrow);
        AddSelectionListeners(rightArrow);
        AddSubmitListeners(nameButton);
        AddCancelListeners(nameButton);
        UpdateCharacterData(currCharacterIdx, 0);

        // Iniciar en estado desactivado
        Deactivate();

        // Suscribirse al evento de conexión de clientes si estamos en modo red
        if (IsNetworked)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse del evento al destruir el objeto
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log($"Panel {transform.GetSiblingIndex()}: OnNetworkSpawn. IsServer={IsServer}, IsClient={IsClient}, IsAvailable={isAvailable}");

        // Si somos el servidor, anunciamos información actualizada a todos
        if (IsServer && !isAvailable)
        {
            BroadcastPanelInfoToAllClientRpc(transform.GetSiblingIndex(), playerIndex, currCharacterIdx, isSelected);
        }

        // Si somos un cliente (incluyendo host), solicitar información de todos los paneles al servidor
        if (IsClient)
        {
            StartCoroutine(RequestPanelInfoWithDelay());
        }
    }

    private IEnumerator RequestPanelInfoWithDelay()
    {
        // Esperamos un breve momento para asegurarnos de que todo está listo
        yield return new WaitForSeconds(0.5f);

        // Solicitar información de paneles al servidor
        RequestPanelInfoServerRpc(NetworkManager.Singleton.LocalClientId);
        Debug.Log($"Cliente {NetworkManager.Singleton.LocalClientId}: Solicitando información de todos los paneles");
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPanelInfoServerRpc(ulong clientId)
    {
        Debug.Log($"Servidor: Recibida solicitud de información de paneles del cliente {clientId}");

        // Encontrar todos los paneles no disponibles
        CharacterSelectorHandler[] allHandlers = FindObjectsOfType<CharacterSelectorHandler>();
        foreach (var handler in allHandlers)
        {
            if (!handler.isAvailable)
            {
                // Enviar información al cliente sobre este panel ocupado
                BroadcastPanelInfoRpc(clientId, handler.transform.GetSiblingIndex(), handler.playerIndex, handler.CurrentCharacterIndex, handler.isSelected);
                Debug.Log($"Servidor: Enviando información del panel {handler.transform.GetSiblingIndex()} al cliente {clientId}");
            }
        }
    }

    // Se llama cuando un cliente se conecta al juego
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Panel {transform.GetSiblingIndex()}: Cliente conectado con ID {clientId}");

        // Si somos el servidor y este panel ya está asignado, informar al cliente
        if (IsServer && !isAvailable)
        {
            // Notificar al nuevo cliente sobre el estado actual del panel
            BroadcastPanelInfoRpc(clientId, transform.GetSiblingIndex(), playerIndex, currCharacterIdx, isSelected);
            Debug.Log($"Servidor: Notificando al cliente {clientId} sobre el panel {transform.GetSiblingIndex()}");
        }
    }

    [ClientRpc]
    private void BroadcastPanelInfoToAllClientRpc(int panelSiblingIndex, int playerIdx, int characterIdx, bool selected)
    {
        Debug.Log($"Recibida info global del panel {panelSiblingIndex}, jugador {playerIdx}, personaje {characterIdx}");

        // No procesar si este es nuestro panel controlado localmente
        if (transform.GetSiblingIndex() == panelSiblingIndex && canControl)
            return;

        // Buscar el panel correspondiente al índice en la jerarquía
        CharacterSelectorHandler[] allPanels = FindObjectsOfType<CharacterSelectorHandler>();
        CharacterSelectorHandler targetPanel = null;

        foreach (var panel in allPanels)
        {
            if (panel.transform.GetSiblingIndex() == panelSiblingIndex)
            {
                targetPanel = panel;
                break;
            }
        }

        if (targetPanel == null)
        {
            Debug.LogWarning($"No se encontró panel con índice {panelSiblingIndex}");
            return;
        }

        // Si el panel pertenece a un jugador local, no modificarlo
        if (targetPanel.canControl)
        {
            Debug.Log($"Panel {panelSiblingIndex} pertenece al jugador local, no se modifica");
            return;
        }

        // Configurar el panel como ocupado
        targetPanel.isAvailable = false;
        targetPanel.playerIndex = playerIdx;
        targetPanel.currCharacterIdx = characterIdx;
        targetPanel.isSelected = selected;

        // Activar paneles visuales
        targetPanel.portraitPanel.gameObject.SetActive(true);
        targetPanel.emptyPanel.gameObject.SetActive(false);

        // Actualizar visualmente
        targetPanel.UpdateCharacterData(0, characterIdx);

        // Actualizar flechas según estado de selección
        if (selected)
        {
            targetPanel.leftArrow.gameObject.SetActive(false);
            targetPanel.rightArrow.gameObject.SetActive(false);
            targetPanel.portraitOutline.enabled = true;
        }
        else
        {
            targetPanel.leftArrow.gameObject.SetActive(true);
            targetPanel.rightArrow.gameObject.SetActive(true);
            targetPanel.portraitOutline.enabled = false;
        }

        Debug.Log($"Panel {panelSiblingIndex} actualizado con personaje {characterIdx}");
    }

    [Rpc(SendTo.Everyone)]
    private void BroadcastPanelInfoRpc(ulong targetClientId, int panelSiblingIndex, int playerIdx, int characterIdx, bool selected)
    {
        // Solo el cliente objetivo debe procesar esta notificación
        if (NetworkManager.Singleton.LocalClientId != targetClientId)
            return;

        Debug.Log($"Cliente {targetClientId}: Recibida info del panel {panelSiblingIndex}");

        // Buscar el panel correspondiente al índice en la jerarquía
        CharacterSelectorHandler[] allPanels = FindObjectsOfType<CharacterSelectorHandler>();
        CharacterSelectorHandler targetPanel = null;

        foreach (var panel in allPanels)
        {
            if (panel.transform.GetSiblingIndex() == panelSiblingIndex)
            {
                targetPanel = panel;
                break;
            }
        }

        if (targetPanel == null)
        {
            Debug.LogWarning($"Cliente {targetClientId}: No se encontró panel con índice {panelSiblingIndex}");
            return;
        }

        // Si el panel pertenece a un jugador local, no modificarlo
        if (targetPanel.pi != null && targetPanel.pi.GetComponent<Player>().IsLocalPlayer)
        {
            Debug.Log($"Cliente {targetClientId}: Panel {panelSiblingIndex} pertenece al jugador local, no se modifica");
            return;
        }

        // Configurar el panel como ocupado
        targetPanel.isAvailable = false;
        targetPanel.playerIndex = playerIdx;
        targetPanel.currCharacterIdx = characterIdx;
        targetPanel.isSelected = selected;

        // Activar paneles visuales
        targetPanel.portraitPanel.gameObject.SetActive(true);
        targetPanel.emptyPanel.gameObject.SetActive(false);

        // Actualizar visualmente
        targetPanel.UpdateCharacterData(0, characterIdx);

        // Actualizar flechas según estado de selección
        if (selected)
        {
            targetPanel.leftArrow.gameObject.SetActive(false);
            targetPanel.rightArrow.gameObject.SetActive(false);
            targetPanel.portraitOutline.enabled = true;
        }
        else
        {
            targetPanel.leftArrow.gameObject.SetActive(true);
            targetPanel.rightArrow.gameObject.SetActive(true);
            targetPanel.portraitOutline.enabled = false;
        }

        Debug.Log($"Cliente {targetClientId}: Panel {panelSiblingIndex} actualizado con personaje {characterIdx}");
    }

    public void Activate(PlayerInput pi)
    {
        if (IsNetworked)
        {
            var netObj = pi.GetComponent<Player>().GetNetworkObject();
            if (netObj != null && netObj.IsSpawned)
            {
                // Determinar si este panel pertenece al jugador local
                canControl = pi.GetComponent<Player>().IsLocalPlayer;
                this.pi = pi;
                this.playerIndex = pi.playerIndex;
                this.isAvailable = false;

                // Activar visualmente el panel
                portraitPanel.gameObject.SetActive(true);
                emptyPanel.gameObject.SetActive(false);

                // Enviar RPC para activar el panel en todos los clientes
                ActivateRpc(transform.GetSiblingIndex(), pi.playerIndex, 0);

                Debug.Log($"Panel {transform.GetSiblingIndex()}: activado para jugador {pi.playerIndex}, canControl={canControl}");
            }
            else
            {
                Debug.LogWarning("Intentando activar un panel con un NetworkObject no spawneado.");
            }
        }
        else
        {
            isAvailable = false;
            portraitPanel.gameObject.SetActive(true);
            emptyPanel.gameObject.SetActive(false);
            this.playerIndex = pi.playerIndex;
            this.pi = pi;
            canControl = true; // En modo local, siempre podemos controlar
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ActivateRpc(int panelSiblingIndex, int playerIndex, int initialCharacterIdx)
    {
        Debug.Log($"Recibida activación para panel {panelSiblingIndex}, jugador {playerIndex}");

        // No procesar si este es nuestro panel
        if (transform.GetSiblingIndex() == panelSiblingIndex && canControl)
            return;

        // Buscar el panel correspondiente
        CharacterSelectorHandler[] allPanels = FindObjectsOfType<CharacterSelectorHandler>();
        CharacterSelectorHandler targetPanel = null;

        foreach (var panel in allPanels)
        {
            if (panel.transform.GetSiblingIndex() == panelSiblingIndex)
            {
                targetPanel = panel;
                break;
            }
        }

        if (targetPanel == null)
        {
            Debug.LogWarning($"No se encontró panel con índice {panelSiblingIndex}");
            return;
        }

        // Si el panel ya está siendo controlado localmente, ignorar
        if (targetPanel.canControl)
        {
            Debug.Log($"Panel {panelSiblingIndex} ya está controlado localmente");
            return;
        }

        // Configurar el panel para un jugador remoto
        targetPanel.isAvailable = false;
        targetPanel.playerIndex = playerIndex;
        targetPanel.canControl = false;

        // Activar visualmente el panel
        targetPanel.portraitPanel.gameObject.SetActive(true);
        targetPanel.emptyPanel.gameObject.SetActive(false);

        // Actualizar visualización
        targetPanel.UpdateCharacterData(0, initialCharacterIdx);

        Debug.Log($"Panel {panelSiblingIndex} activado para jugador remoto {playerIndex}");
    }

    public void Deactivate()
    {
        isAvailable = true;
        portraitPanel.gameObject.SetActive(false);
        emptyPanel.gameObject.SetActive(true);
        playerIndex = -1;
        canControl = false;
        pi = null;
    }

    void Update()
    {
        if (!isSelected && pcm.lockedCharacterData[currCharacterIdx])
        {
            overlay.gameObject.SetActive(true);
        }
        else
        {
            overlay.gameObject.SetActive(false);
        }
    }

    // Agregar listeners para eventos
    // --------------------------------------------------------------------------------

    protected virtual void AddSubmitListeners(Selectable selectable)
    {
        EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = selectable.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry SubmitEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Submit
        };
        SubmitEntry.callback.AddListener(OnSubmit);
        trigger.triggers.Add(SubmitEntry);
    }

    protected virtual void AddCancelListeners(Selectable selectable)
    {
        EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = selectable.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry CancelEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Cancel
        };
        CancelEntry.callback.AddListener(OnCancel);
        trigger.triggers.Add(CancelEntry);
    }

    protected virtual void AddSelectionListeners(Selectable selectable)
    {
        EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = selectable.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry SelectEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Select
        };
        SelectEntry.callback.AddListener(OnSelect);
        trigger.triggers.Add(SelectEntry);
    }

    // Callbacks para eventos de UI
    // --------------------------------------------------------------------------------

    public void OnSelect(BaseEventData eventData)
    {
        // Solo permitir control si este panel pertenece al jugador local
        if (!canControl || isAvailable || isSelected)
            return;

        if (eventSystem.currentSelectedGameObject == leftArrow.gameObject)
        {
            HandleArrow(-1);

            // En modo red, notificar cambio de selección
            if (IsNetworked)
            {
                NotifyCharacterSelectionChangedServerRpc(transform.GetSiblingIndex(), playerIndex, currCharacterIdx);
            }
        }
        else if (eventSystem.currentSelectedGameObject == rightArrow.gameObject)
        {
            HandleArrow(1);

            // En modo red, notificar cambio de selección
            if (IsNetworked)
            {
                NotifyCharacterSelectionChangedServerRpc(transform.GetSiblingIndex(), playerIndex, currCharacterIdx);
            }
        }

        Invoke(nameof(ResetSelection), 0.1f);
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyCharacterSelectionChangedServerRpc(int panelSiblingIndex, int playerIdx, int characterIdx)
    {
        // El servidor recibe la notificación y la reenvía a todos los clientes
        NotifyCharacterSelectionChangedClientRpc(panelSiblingIndex, playerIdx, characterIdx);
    }

    [ClientRpc]
    private void NotifyCharacterSelectionChangedClientRpc(int panelSiblingIndex, int playerIdx, int characterIdx)
    {
        Debug.Log($"Cambio de selección en panel {panelSiblingIndex}, personaje {characterIdx}");

        // No modificar si este es nuestro panel controlado localmente
        if (transform.GetSiblingIndex() == panelSiblingIndex && canControl)
            return;

        // Buscar el panel en la jerarquía
        CharacterSelectorHandler[] allPanels = FindObjectsOfType<CharacterSelectorHandler>();
        CharacterSelectorHandler targetPanel = null;

        foreach (var panel in allPanels)
        {
            if (panel.transform.GetSiblingIndex() == panelSiblingIndex)
            {
                targetPanel = panel;
                break;
            }
        }

        if (targetPanel == null || targetPanel.canControl)
            return;

        // Actualizar la visualización del personaje para paneles remotos
        targetPanel.UpdateCharacterData(0, characterIdx);
    }

    public void OnCancel(BaseEventData eventData)
    {
        // Solo permitir control si este panel pertenece al jugador local
        if (!canControl || isAvailable)
            return;

        if (!isSelected)
        {
            if (playerIndex == 0)
            {
                pcm.BackToMainMenu();
            }
            return;
        }

        portraitPanel.DOKill(true);
        pcm.UnlockCharacter(currCharacterIdx, pi);
        isSelected = false;
        portraitPanel.DOPunchScale(new Vector3(-scaleFactor, -scaleFactor, -scaleFactor), 0.2f, 5, 1);
        portraitOutline.enabled = false;
        AudioManager.PlaySound(cancelSoundID);
        leftArrow.gameObject.SetActive(true);
        rightArrow.gameObject.SetActive(true);

        // En modo red, notificar sobre el desbloqueo
        if (IsNetworked)
        {
            NotifyCharacterUnlockedServerRpc(transform.GetSiblingIndex(), playerIndex, currCharacterIdx);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyCharacterUnlockedServerRpc(int panelSiblingIndex, int playerIdx, int characterIdx)
    {
        // El servidor recibe la notificación y la reenvía a todos los clientes
        NotifyCharacterUnlockedClientRpc(panelSiblingIndex, playerIdx, characterIdx);
    }

    [ClientRpc]
    private void NotifyCharacterUnlockedClientRpc(int panelSiblingIndex, int playerIdx, int characterIdx)
    {
        Debug.Log($"Desbloqueo en panel {panelSiblingIndex}, personaje {characterIdx}");

        // No modificar si este es nuestro panel controlado localmente
        if (transform.GetSiblingIndex() == panelSiblingIndex && canControl)
            return;

        // Buscar el panel en la jerarquía
        CharacterSelectorHandler[] allPanels = FindObjectsOfType<CharacterSelectorHandler>();
        CharacterSelectorHandler targetPanel = null;

        foreach (var panel in allPanels)
        {
            if (panel.transform.GetSiblingIndex() == panelSiblingIndex)
            {
                targetPanel = panel;
                break;
            }
        }

        if (targetPanel == null || targetPanel.canControl)
            return;

        // Actualizar la visualización del personaje para paneles remotos
        targetPanel.isSelected = false;
        targetPanel.leftArrow.gameObject.SetActive(true);
        targetPanel.rightArrow.gameObject.SetActive(true);
        targetPanel.portraitOutline.enabled = false;
    }

    public void OnSubmit(BaseEventData eventData)
    {
        // Solo permitir control si este panel pertenece al jugador local
        if (!canControl || isAvailable)
            return;

        if (isSelected)
        {
            if (playerIndex == 0)
            {
                pcm.StartGame();
            }
            return;
        }

        portraitPanel.DOKill(true);
        if (pcm.lockedCharacterData[currCharacterIdx])
        {
            portraitPanel.DOPunchScale(new Vector3(scaleFactor, scaleFactor, scaleFactor), 0.2f, 5, 1);
            AudioManager.PlaySound(denySoundID);
        }
        else
        {
            bool selectionOk = pcm.LockCharacter(currCharacterIdx, pi);
            isSelected = selectionOk;
            AudioManager.PlaySound(selectionSoundID);
            portraitPanel.DOPunchScale(new Vector3(scaleFactor, scaleFactor, scaleFactor), 0.5f, 10, 1);
            leftArrow.gameObject.SetActive(false);
            rightArrow.gameObject.SetActive(false);
            portraitOutline.enabled = true;

            // En modo red, notificar sobre el bloqueo
            if (IsNetworked)
            {
                NotifyCharacterLockedServerRpc(transform.GetSiblingIndex(), playerIndex, currCharacterIdx);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyCharacterLockedServerRpc(int panelSiblingIndex, int playerIdx, int characterIdx)
    {
        // El servidor recibe la notificación y la reenvía a todos los clientes
        NotifyCharacterLockedClientRpc(panelSiblingIndex, playerIdx, characterIdx);
    }

    [ClientRpc]
    private void NotifyCharacterLockedClientRpc(int panelSiblingIndex, int playerIdx, int characterIdx)
    {
        Debug.Log($"Bloqueo en panel {panelSiblingIndex}, personaje {characterIdx}");

        // No modificar si este es nuestro panel controlado localmente
        if (transform.GetSiblingIndex() == panelSiblingIndex && canControl)
            return;

        // Buscar el panel en la jerarquía
        CharacterSelectorHandler[] allPanels = FindObjectsOfType<CharacterSelectorHandler>();
        CharacterSelectorHandler targetPanel = null;

        foreach (var panel in allPanels)
        {
            if (panel.transform.GetSiblingIndex() == panelSiblingIndex)
            {
                targetPanel = panel;
                break;
            }
        }

        if (targetPanel == null || targetPanel.canControl)
            return;

        // Actualizar la visualización del personaje para paneles remotos
        targetPanel.UpdateCharacterData(0, characterIdx);
        targetPanel.isSelected = true;
        targetPanel.leftArrow.gameObject.SetActive(false);
        targetPanel.rightArrow.gameObject.SetActive(false);
        targetPanel.portraitOutline.enabled = true;
    }

    // Métodos auxiliares
    // --------------------------------------------------------------------------------

    private void HandleArrow(int direction)
    {
        if (isSelected) return;
        if (DOTween.IsTweening(portraitPanel))
        {
            DOTween.Kill(portraitPanel, true);
            portraitPanel.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        // Determinar el ángulo de sacudida según la dirección
        float angle = direction > 0 ? -shakeAngle : shakeAngle;
        portraitPanel.DOPunchRotation(new Vector3(0, 0, angle), shakeDuration, 5, 1);
        portraitPanel.DOPunchScale(new Vector3(scaleFactor, scaleFactor, scaleFactor), 0.5f, 10, 1);

        // Actualizar el índice actual según la dirección
        UpdateCharacterData(currCharacterIdx, direction);

        // Reproducir efecto de sonido
        AudioManager.PlaySound("FX_ChangeCharacter");
    }

    public void UpdateCharacterData(int index, int direction)
    {
        bool fetchOk = pcm.GetCharacterData(index, direction, out CharacterData data, out int newIndex, out bool isLocked);

        if (!fetchOk)
        {
            Debug.LogError("Failed to fetch character data.");
            return;
        }

        currCharacterIdx = newIndex;
        currCharacterData = data;

        if (isLocked) overlay.gameObject.SetActive(true);
        else overlay.gameObject.SetActive(false);

        portraitImage.color = new Color(data.portraitLuminosity, data.portraitLuminosity, data.portraitLuminosity, 1.0f);
        portraitImage.sprite = data.portrait;
        portraitImage.GetComponent<Animator>().runtimeAnimatorController = data.portraitAnimator;
        nameImage.sprite = data.text;
        nameImage.GetComponent<Outline>().effectColor = data.textOutlineColor;
    }

    private void ResetSelection()
    {
        eventSystem.SetSelectedGameObject(nameButton.gameObject);
    }

    // Método público para otras clases
    public void UpdateVisualState(bool locked)
    {
        if (locked)
        {
            isSelected = true;
            leftArrow.gameObject.SetActive(false);
            rightArrow.gameObject.SetActive(false);
            portraitOutline.enabled = true;
        }
        else
        {
            isSelected = false;
            leftArrow.gameObject.SetActive(true);
            rightArrow.gameObject.SetActive(true);
            portraitOutline.enabled = false;
        }
    }
}
