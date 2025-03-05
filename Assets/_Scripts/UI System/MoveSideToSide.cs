using UnityEngine;
using DG.Tweening;

public class MoveUIElement : MonoBehaviour
{
    public RectTransform rectTransform; // Asigna el RectTransform en el Inspector
    public float distance = 100f; // Distancia en p�xeles
    public float duration = 2f; // Duraci�n de la transici�n

    void Start()
    {
        Move();
    }

    void Move()
    {
        float startX = rectTransform.anchoredPosition.x;

        rectTransform.DOAnchorPosX(startX + distance, duration)
            .SetEase(Ease.InOutSine) // Movimiento suave
            .SetLoops(-1, LoopType.Yoyo); // Movimiento infinito de lado a lado
    }
}
