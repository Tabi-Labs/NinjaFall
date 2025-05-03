using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInit : MonoBehaviour
{
    [SerializeField] string songId = "song_id";

    void Start()
    {
        AudioManager.PlayMusic(songId);
    }

}
