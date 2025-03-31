using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class CharacterSelectorHandler : MonoBehaviour
{

    private RectTransform portraitPanel;
    private RectTransform emptyPanel;
    private Selectable nameButton;
    private Selectable leftArrow;
    private Selectable rightArrow;
    private MultiplayerEventSystem eventSystem;
    private Image portraitImage;
    private Image nameImage;
    private PlayerConfigurationManager pcm;

    [Header("Visual Settings")]
    [SerializeField] private float shakeAngle = 3.0f;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float scaleFactor = 0.06f;
    [SerializeField] private float luminosityFactor = 0.3f;

    private CharacterData currCharacterData;
    
    public bool isAvailable {get; private set;} = true;
    public bool isSelected {get; private set;} = false;

    private int currCharacterIdx = 0;
   
    // Start is called before the first frame update
    void Start()
    {
        portraitPanel = transform.Find("PortraitPanel").GetComponent<RectTransform>();
        emptyPanel = transform.Find("EmptyPanel").GetComponent<RectTransform>();
        leftArrow = transform.Find("PortraitPanel/LeftArrow").GetComponent<Selectable>();
        rightArrow = transform.Find("PortraitPanel/RightArrow").GetComponent<Selectable>();
        eventSystem = transform.Find("EventSystem").GetComponent<MultiplayerEventSystem>();
        portraitImage = transform.Find("PortraitPanel/Portrait").GetComponent<Image>();
        
        Transform text = transform.Find("PortraitPanel/Name");
        nameButton = text.GetComponent<Selectable>();
        nameImage = nameButton.GetComponent<Image>();
        
        pcm = PlayerConfigurationManager.Instance;
        
        AddSelectionListeners(leftArrow);
        AddSelectionListeners(rightArrow);
        AddSubmitListeners(nameButton);
        UpdateCharacterData(currCharacterIdx, 0);
        
        Deactivate();
    }

    void Update()
    {
        // Each frame check if current character has been selected

    }

    public void NotifyLock(int index, bool isLocked){
        // Update Current Color if locked
        if (index == currCharacterIdx)
        {
            float luminosity = currCharacterData.portraitLuminosity - (isLocked ? luminosityFactor : 0);
            float nameLum = 1 - (isLocked ? luminosityFactor * 2 : 0);
            portraitImage.color = new Color(luminosity, luminosity, luminosity, 1);
            nameImage.color = new Color(nameLum, nameLum, nameLum, 1);
        }
    }

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

    public void OnSelect(BaseEventData eventData)
    {
        if (eventSystem.currentSelectedGameObject == leftArrow.gameObject)
        {
            HandleArrow(-1); // -1 for left arrow
        }
        else if (eventSystem.currentSelectedGameObject == rightArrow.gameObject)
        {
            HandleArrow(1); // 1 for right arrow
        }

        // Delay the selection reset slightly
        Invoke(nameof(ResetSelection), 0.1f);
    }

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

    public void OnSubmit(BaseEventData eventData){
        if (isSelected) return;
        bool selectionOk = pcm.LockCharacter(currCharacterIdx);
        isSelected = selectionOk;
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

        float portraitLum = data.portraitLuminosity - (isLocked ? luminosityFactor : 0);
        float nameLum = 1 - (isLocked ? luminosityFactor*2 : 0);

        portraitImage.sprite = data.portrait;
        portraitImage.color = new Color(portraitLum, portraitLum, portraitLum, 1);
        nameImage.color = new Color(nameLum, nameLum, nameLum, 1);
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

    public void Activate()
    {
        isAvailable = false;
        portraitPanel.gameObject.SetActive(true);
        emptyPanel.gameObject.SetActive(false);
    }

    public void Deactivate()
    {
        isAvailable = true;
        portraitPanel.gameObject.SetActive(false);
        emptyPanel.gameObject.SetActive(true);
    }

}
