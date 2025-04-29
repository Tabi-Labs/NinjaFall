using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class StatusEffectSpawner : MonoBehaviour
{
    [Header("Animation Settings")]
    public float moveFactor = 0.15f;
    public float animationDuration = 2f;

    public GameObject[] buffPrefabs; // Lista de buffs prefabs
    public Transform[] spawnPoints; // Posiciones donde pueden aparecer los buffs
    public float spawnInterval = 5f; // Intervalo de aparici�n en segundos

    private GameObject[] activeBuffs; // Control de buffs activos en cada spot

    private float globalFloatValue = 0f;
    private Tweener globalFloatTween;

    // Start is called before the first frame update
    void Start()
    {
        activeBuffs = new GameObject[spawnPoints.Length];

        globalFloatTween = DOTween.To(() => globalFloatValue, x => globalFloatValue = x, 1f, animationDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        StartCoroutine(SpawnBuffs());
    }

    void Update(){
        // Efecto de latido global
        for (int i = 0; i < activeBuffs.Length; i++)
        {
            if (activeBuffs[i] != null)
            {
                Vector3 pos = spawnPoints[i].position + Vector3.up * globalFloatValue * moveFactor;
                activeBuffs[i].transform.position = pos;
            }
        }
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

        if (activeBuffs[spawnPointIndex] == null || 
            (activeBuffs[spawnPointIndex].GetComponent<SpriteRenderer>() != null && !activeBuffs[spawnPointIndex].GetComponent<SpriteRenderer>().enabled))
        {
            GameObject buffPrefab = buffPrefabs[Random.Range(0, buffPrefabs.Length)];
            Transform spawnPoint = spawnPoints[spawnPointIndex];

            // Instancia el buff
            GameObject buff = Instantiate(buffPrefab, spawnPoint.position - Vector3.up * 0.5f, Quaternion.identity);
            activeBuffs[spawnPointIndex] = buff;

            // Animación de entrada
            SpriteRenderer spriteRenderer = buff.GetComponent<SpriteRenderer>();
            buff.GetComponent<SpriteRenderer>().color = new Vector4(1, 1, 1, 0f);
            spriteRenderer.DOFade(1f, 3f).SetEase(Ease.OutFlash);
        }
        
    }

}
