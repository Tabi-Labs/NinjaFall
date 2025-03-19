using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using DG.Tweening;

public class ChracterSelectorEventSystemHandler : MonoBehaviour
{

    // References
    private RectTransform panel;
    private Selectable textButton;
    private Selectable leftArrow;
    private Selectable rightArrow;
    private MultiplayerEventSystem eventSystem;
    private PlayerConfigurationManager pcm;
    private Image portraitImage;
    private Image textImage;

    private int currentIndex = 0;

    [SerializeField]
    private CharacterData[] characterDatas;

    [Header("Tweening Variables")]
    [SerializeField]
    private float shakeAngle = 3.0f;
    private float shakeDuration = 0.3f;
    private float scaleFactor = 0.06f;
    
    // Start is called before the first frame update
    void Start()
    {
        panel = GameObject.Find("PortraitPanel").GetComponent<RectTransform>();
        leftArrow = GameObject.Find("PortraitPanel/LeftArrow").GetComponent<Selectable>();
        rightArrow = GameObject.Find("PortraitPanel/RightArrow").GetComponent<Selectable>();
        eventSystem = GameObject.Find("EventSystem").GetComponent<MultiplayerEventSystem>();
        portraitImage = GameObject.Find("PortraitPanel/Portrait").GetComponent<Image>();
        
        GameObject text = GameObject.Find("PortraitPanel/Name");
        textButton = text.GetComponent<Selectable>();
        textImage = text.GetComponent<Image>();
        
        pcm = PlayerConfigurationManager.Instance;
        
        AddSelectionListeners(leftArrow);
        AddSelectionListeners(rightArrow);
        AddSubmitListeners(textButton);
        UpdateCharacterData(characterDatas[currentIndex]);
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
        if (DOTween.IsTweening(panel))
        {
            DOTween.Kill(panel, true);
            panel.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        // Determine the shake angle based on direction
        float angle = direction > 0 ? -shakeAngle : shakeAngle;
        panel.DOPunchRotation(new Vector3(0, 0, angle), shakeDuration, 5, 1);
        panel.DOPunchScale(new Vector3(scaleFactor, scaleFactor, scaleFactor), 0.5f, 10, 1);

        // Update the current index based on direction
        currentIndex = (currentIndex + direction + characterDatas.Length) % characterDatas.Length;
        UpdateCharacterData(characterDatas[currentIndex]);

        // Play sound effect
        AudioManager.PlaySound("FX_ChangeCharacter");
    }

    public void OnSubmit(BaseEventData eventData){
        Debug.Log("Submit");
        AudioManager.PlaySound("FX_ChooseCharacter");
    }

    public void UpdateCharacterData(CharacterData data)
    {
        portraitImage.sprite = data.portrait;
        portraitImage.color = new Color(data.portraitLuminosity, data.portraitLuminosity, data.portraitLuminosity, 1);
        portraitImage.GetComponent<Animator>().runtimeAnimatorController = data.portraitAnimator;
        // portraitImage.material = data.mat;
        textImage.sprite = data.text;
        textImage.material = data.mat;
        textImage.GetComponent<Outline>().effectColor = data.textOutlineColor;
    }

    private void ResetSelection()
    {
        eventSystem.SetSelectedGameObject(textButton.gameObject);
    }

}
