using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXHandler : MonoBehaviour
{
    [SerializeField] private VFXDict vfxDict;

    void Awake()
    {
        vfxDict.InitializeList();
    }
}
