using UnityEngine;

public class SpinObject : MonoBehaviour
{
    public float spinSpeed = 100f; // Adjust rotation speed

    void Update()
    {
        transform.Rotate(Vector3.forward * spinSpeed * Time.deltaTime);
    }
}
