using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMusicHandler : MonoBehaviour
{
    void Start()
    {
        AudioManager.PlayMusic("MX_Tutorial");
    }
}
