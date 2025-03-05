using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PointerController : MonoBehaviour
{
    public RectTransform pointer;
    private Vector2 lastPos;

    [Header("Positioning")]
    public float pointerOffset = 10f; // Distancia entre el puntero y el objeto seleccionado
    public RectTransform parentPanel; // Panel que contiene los botones
    private Vector2 parentPos;

    [Header("Tweening")]
    public float distance = 20f; // Distancia en píxeles
    public float duration = 2f;   // Duración de la transición

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = pointer.GetComponent<RectTransform>();
        parentPos = parentPanel != null ? new Vector2(parentPanel.rect.min.x, parentPanel.anchoredPosition.y) : Vector2.zero;
    }

    void Update()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected != null && selected.TryGetComponent<RectTransform>(out RectTransform selectedRect))
        {
            Vector2 pos = selectedRect.anchoredPosition;
            if(lastPos == pos) return;
            MovePointer(selectedRect);
            lastPos = pos;
        }
    }

    public void OnButtonHover(GameObject button)
    {
        if (button.TryGetComponent<RectTransform>(out RectTransform buttonRect))
        {
            EventSystem.current.SetSelectedGameObject(button);
            MovePointer(buttonRect);
        }
    }

    private void MovePointer(RectTransform target)
    {
        DOTween.Kill(rectTransform);
        rectTransform.anchoredPosition = parentPos + target.anchoredPosition - new Vector2(pointerOffset - target.rect.min.x, 0);
        Tween();
    }

    private void Tween(){
        float startX = rectTransform.anchoredPosition.x;
        rectTransform.DOAnchorPosX(startX - distance, duration)
            .SetEase(Ease.InOutSine) // Movimiento suave
            .SetLoops(-1, LoopType.Yoyo); // Movimiento infinito de lado a lado
    }
}
