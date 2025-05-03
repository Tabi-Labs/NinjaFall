using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelector : MonoBehaviour
{
    public GameObject[] levelPrefabs;

    void Start()
    {
        // Choose a random level prefab from the list
        int randomIndex = Random.Range(0, levelPrefabs.Length);

        // Insantiate the chosen level prefab
        for (int i = 0; i < levelPrefabs.Length; i++)
        {
            if (i == randomIndex)
            {
                Instantiate(levelPrefabs[i], Vector3.zero, Quaternion.identity);
            }                
        }
    }
}
