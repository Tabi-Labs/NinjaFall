using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniquePersistentGroup : MonoBehaviour
{

    // Singleton Pattern
    // --------------------------------------------------------------------------------
    public static UniquePersistentGroup Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("[Singleton] Trying to instantiate a second instance of a singleton class.");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

}
