using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using DG.Tweening;
using System;
using TMPro;
using UnityEditor.SearchService;

public class CharacterSelectorHandler : MonoBehaviour
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

    // References
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

    // Properties
    public bool isAvailable {get; private set;} = true;
    public bool isSelected {get; private set;} = false;
    public int playerIndex {get; private set;} = -1;

    // Private Variables
    private int currCharacterIdx = 0;
    private CharacterData currCharacterData;

    // Initialization and Setup
    // --------------------------------------------------------------------------------

    void Start()
    {
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

        AddSelectionListeners(leftArrow);
        AddSelectionListeners(rightArrow);
        AddSubmitListeners(nameButton);
        AddCancelListeners(nameButton);
        UpdateCharacterData(currCharacterIdx, 0);

        Deactivate();
    }

    public void Activate(int playerIndex)
    {
        StartCoroutine(SetAvailabilityNextFrame(false));
        portraitPanel.gameObject.SetActive(true);
        emptyPanel.gameObject.SetActive(false);
        this.playerIndex = playerIndex;
    }

    private IEnumerator SetAvailabilityNextFrame(bool available)
    {
        yield return null;
        isAvailable = available;
    }

    public void Deactivate()
    {
        isAvailable = true;
        portraitPanel.gameObject.SetActive(false);
        emptyPanel.gameObject.SetActive(true);
        playerIndex = -1;
    }


    void Update()
    {
        if(!isSelected && pcm.lockedCharacterData[currCharacterIdx])
        {
            overlay.gameObject.SetActive(true);
        }
        else
        {
            overlay.gameObject.SetActive(false);
        }
    }

    /// Add Event Trigger Listeners
    /// --------------------------------------------------------------------------------

    protected virtual void AddSubmitListeners(Selectable selectable){
        EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = selectable.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry SubmitEntry = new EventTrigger.Entry{
            eventID = EventTriggerType.Submit
        };
        SubmitEntry.callback.AddListener(OnSubmit);
        trigger.triggers.Add(SubmitEntry);
    }

    protected virtual void AddCancelListeners(Selectable selectable){
        EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = selectable.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry CancelEntry = new EventTrigger.Entry{
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

        EventTrigger.Entry SelectEntry = new EventTrigger.Entry{
            eventID = EventTriggerType.Select
        };
        SelectEntry.callback.AddListener(OnSelect);
        trigger.triggers.Add(SelectEntry);
    }

    /// Event Trigger Callbacks
    /// --------------------------------------------------------------------------------

    public void OnSelect(BaseEventData eventData)
    {
        if (eventSystem.currentSelectedGameObject == leftArrow.gameObject)
        {
            HandleArrow(-1);
        }
        else if (eventSystem.currentSelectedGameObject == rightArrow.gameObject)
        {
            HandleArrow(1);
        }

        Invoke(nameof(ResetSelection), 0.1f);
    }

    public void OnCancel(BaseEventData eventData){
        if (!isSelected){
            if(playerIndex == 0){
                pcm.BackToMainMenu();
            }
        }
        if (isAvailable) return;
        portraitPanel.DOKill(true);
        pcm.UnlockCharacter(currCharacterIdx);
        isSelected = false;
        portraitPanel.DOPunchScale(new Vector3(-scaleFactor, -scaleFactor, -scaleFactor), 0.2f, 5, 1);
        portraitOutline.enabled = false;
        AudioManager.PlaySound(cancelSoundID);
        leftArrow.gameObject.SetActive(true);
        rightArrow.gameObject.SetActive(true);
    }

    public void OnSubmit(BaseEventData eventData){
        if (isAvailable) return;
        if (isSelected) {
            if (playerIndex == 0){
                pcm.StartGame();
            }
            return;
        };
        portraitPanel.DOKill(true);
        if (pcm.lockedCharacterData[currCharacterIdx]){
            portraitPanel.DOPunchScale(new Vector3(scaleFactor, scaleFactor, scaleFactor), 0.2f, 5, 1);
            AudioManager.PlaySound(denySoundID);
        } else {
            bool selectionOk = pcm.LockCharacter(currCharacterIdx);
            isSelected = selectionOk;
            AudioManager.PlaySound(selectionSoundID);
            portraitPanel.DOPunchScale(new Vector3(scaleFactor, scaleFactor, scaleFactor), 0.5f, 10, 1);
            leftArrow.gameObject.SetActive(false);
            rightArrow.gameObject.SetActive(false);
            portraitOutline.enabled = true;
        }
    }

    // Auxiliary Methods
    // --------------------------------------------------------------------------------

    private void HandleArrow(int direction)
    {
        if (isSelected) return;
        if (DOTween.IsTweening(portraitPanel))
        {
            DOTween.Kill(portraitPanel, true);
            portraitPanel.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        // Determine the shake angle based on direction
        float angle = direction > 0 ? -shakeAngle : shakeAngle;
        portraitPanel.DOPunchRotation(new Vector3(0, 0, angle), shakeDuration, 5, 1);
        portraitPanel.DOPunchScale(new Vector3(scaleFactor, scaleFactor, scaleFactor), 0.5f, 10, 1);

        // Update the current index based on direction
        UpdateCharacterData(currCharacterIdx, direction);

        // Play sound effect
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

        if(isLocked) overlay.gameObject.SetActive(true);
        else overlay.gameObject.SetActive(false);

        portraitImage.color = new Color(data.portraitLuminosity, data.portraitLuminosity, data.portraitLuminosity, 1.0f);
        portraitImage.sprite = data.portrait;
        portraitImage.GetComponent<Animator>().runtimeAnimatorController = data.portraitAnimator;
        // portraitImage.material = data.mat;
        nameImage.sprite = data.text;
        nameImage.material = data.mat;
        nameImage.GetComponent<Outline>().effectColor = data.textOutlineColor;
    }

    private void ResetSelection()
    {
        eventSystem.SetSelectedGameObject(nameButton.gameObject);
    }
}
