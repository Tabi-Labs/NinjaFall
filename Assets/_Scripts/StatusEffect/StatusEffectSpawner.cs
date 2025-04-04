using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectSpawner : MonoBehaviour
{
    public GameObject[] buffPrefabs; // Lista de buffs prefabs
    public Transform[] spawnPoints; // Posiciones donde pueden aparecer los buffs
    public float spawnInterval = 5f; // Intervalo de aparición en segundos


    private GameObject[] activeBuffs; // Control de buffs activos en cada spot


    // Start is called before the first frame update
    void Start()
    {
        activeBuffs = new GameObject[spawnPoints.Length];
        StartCoroutine(SpawnBuffs());

    }

    IEnumerator SpawnBuffs()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnBuff();
        }
    }


    void SpawnBuff()
    {
        if (buffPrefabs.Length == 0 || spawnPoints.Length == 0) return;

        // Selecciona un prefab y un punto de spawn aleatorio
        int spawnPointIndex = Random.Range(0, spawnPoints.Length);

        if (activeBuffs[spawnPointIndex] == null)
        {
            GameObject buffPrefab = buffPrefabs[Random.Range(0, buffPrefabs.Length)];
            Transform spawnPoint = spawnPoints[spawnPointIndex];

            // Instancia el buff
            GameObject buff = Instantiate(buffPrefab, spawnPoint.position, Quaternion.identity);
            activeBuffs[spawnPointIndex] = buff;
        }
        
    }

}
