using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine.SceneManagement;

//TODO - Add support for controller vibration
//TODO - Add cancel functionality to go back to previous menu
//TODO - Add support for bot addition?

public class MenuEventSystemHandler : MonoBehaviour
{
    [Header("References")]
    public List<Selectable> Selectables = new List<Selectable>();
    [SerializeField] protected Selectable _firstSelected;

    [Header("Controls")]
    [SerializeField] protected InputActionReference _navigateReference;

    [Header("Animations")]
    [SerializeField] protected float _selectedAnimationScale = 1.1f;
    [SerializeField] protected float _scaleDuration = 0.25f;
    [SerializeField] protected List<GameObject> _animationExclusions = new List<GameObject>();
    [System.Flags]   protected enum buttonSubmitBehavior {None, Blink, Sound};
    [SerializeField] protected buttonSubmitBehavior _buttonSubmitBehavior = buttonSubmitBehavior.None;

    [Header("Sounds")]
    [SerializeField] protected string _selectedSoundID = "FX_Hover";
    [SerializeField] protected string _submitSoundID = "FX_Confirm";

    protected Dictionary<Selectable, Vector3> _scales = new Dictionary<Selectable, Vector3>();

    protected Selectable _lastSelected;

    protected Tween _scaleUpTween;
    protected Tween _scaleDownTween;

    private bool firstSelection = true;

    public virtual void Awake()
    {
        foreach (var selectable in Selectables)
        {
            AddSelectionListeners(selectable);
            _scales.Add(selectable, selectable.transform.localScale);
        }
    }

    public virtual void OnEnable()
    {
        _navigateReference.action.performed += OnNavigate;

        //ensure all selectables are reset back to original size
        for (int i = 0; i < Selectables.Count; i++)
        {
            Selectables[i].transform.localScale = _scales[Selectables[i]];
        }

        StartCoroutine(SelectAfterDelay());
    }

    protected virtual IEnumerator SelectAfterDelay()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(_firstSelected.gameObject);
    }

    public virtual void OnDisable()
    {
        _navigateReference.action.performed -= OnNavigate;

        _scaleUpTween.Kill(true);
        _scaleDownTween.Kill(true);
    }

    protected virtual async void AddSelectionListeners(Selectable selectable)
    {
        //add listener
        EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = selectable.gameObject.AddComponent<EventTrigger>();
        }

        //add SELECT event
        EventTrigger.Entry SelectEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Select
        };
        SelectEntry.callback.AddListener(OnSelect);
        trigger.triggers.Add(SelectEntry);

        //add DESELECT event
        EventTrigger.Entry DeselectEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Deselect
        };
        DeselectEntry.callback.AddListener(OnDeselect);
        trigger.triggers.Add(DeselectEntry);

        //add BUTTONCLICK event
        if(selectable is Button){
            Button button = selectable as Button;
            UnityEvent originalEvents = button.onClick;
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(async () => await HandleButtonClickAsync(button, originalEvents));
        }

    }

    public void OnSelect(BaseEventData eventData)
    {
        if(firstSelection){
            firstSelection = false;
            return;
        }

        if (_selectedSoundID != null)
        {
            AudioManager.PlaySound(_selectedSoundID);
        }
        _lastSelected = eventData.selectedObject.GetComponent<Selectable>();

        if (_animationExclusions.Contains(eventData.selectedObject))
            return;

        Vector3 newScale = eventData.selectedObject.transform.localScale * _selectedAnimationScale;
        _scaleUpTween = eventData.selectedObject.transform.DOScale(newScale, _scaleDuration);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (_animationExclusions.Contains(eventData.selectedObject))
            return;

        Selectable sel = eventData.selectedObject.GetComponent<Selectable>();
        _scaleDownTween = eventData.selectedObject.transform.DOScale(_scales[sel], _scaleDuration);
    }

    public async Task HandleButtonClickAsync(Button button, UnityEvent originalEvents)
    {
        EventSystem.current.SetSelectedGameObject(null);

        // Sound Behavior
        if(_buttonSubmitBehavior.HasFlag(buttonSubmitBehavior.Sound)){
            AudioManager.PlaySound(_submitSoundID);
        }

        // Blink Behavior
        if(_buttonSubmitBehavior.HasFlag(buttonSubmitBehavior.Blink)){
            TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
            Tween tween = text?.DOFade(0, 0.1f).SetLoops(6, LoopType.Yoyo).SetEase(Ease.InOutFlash);
            await tween.AsyncWaitForCompletion(); // Espera a que acabe el tween
        }

        originalEvents.Invoke();
    }

    public void TempPrint(){
        Debug.Log("Yo voy segundo");
    }

    protected virtual void OnNavigate(InputAction.CallbackContext context)
    {
        if (EventSystem.current.currentSelectedGameObject == null && _lastSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(_lastSelected.gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        
        // Ñapa apocaliptica
        if(sceneName == "05_Character_Select" && NetworkManager.Singleton || sceneName == "01_Start_Menu" && NetworkManager.Singleton)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("02_Multiplayer_Menu", LoadSceneMode.Single);
        }
        else
        {
            SceneLoader.Instance.ChangeScene(sceneName);
        }
        
    }
}