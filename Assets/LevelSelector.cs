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

        // Activate the chosen level prefab and deactivate the others
        for (int i = 0; i < levelPrefabs.Length; i++)
        {
            if (i == randomIndex)
                levelPrefabs[i].SetActive(true);
            else
                levelPrefabs[i].SetActive(false);
        }
    }
}
