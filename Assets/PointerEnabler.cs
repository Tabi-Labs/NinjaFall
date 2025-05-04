using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerEnabler : MonoBehaviour
{
    public bool enablePointer = false;

    void Start()
    {
        if (enablePointer)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}