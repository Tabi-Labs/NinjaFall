using UnityEngine;

public class FlickerMotion : MonoBehaviour
{
    [Header("Motion Settings")]
    public float radius = 0.5f;          // Qué tanto se puede mover
    public float speed = 1.0f;            // Velocidad del movimiento

    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float offsetX = Mathf.Cos(angle) * radius * Random.value;
        float offsetY = Mathf.Sin(angle) * radius * Random.value;

        Vector3 newPos = originalPosition + new Vector3(offsetX, offsetY, 0f);
        transform.localPosition = Vector3.Lerp(transform.localPosition, newPos, Time.deltaTime * speed);
    }
}