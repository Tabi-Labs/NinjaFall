using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoadScript : MonoBehaviour
{

    // Singleton instance
    public static DontDestroyOnLoadScript Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // // Start is called before the first frame update
    // void Start()
    // {
    //     DontDestroyOnLoad(gameObject);
    // }

}
