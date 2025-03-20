using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingManager : MonoBehaviour
{
    private int size = 15;
    public List<GameObject> pool;

    private int index = 0;

    public void InitializePool(GameObject element, Transform parent, int size = 15, string name = "PooledObject_")
    {
        pool = new List<GameObject>();
        this.size = size;


        for (int i = 0; i < this.size; i++) {
            pool.Add(Instantiate(element, parent));

            pool[i].SetActive(false);
        }
    }

    public GameObject GetObject() { 
        GameObject gO = pool[index];
        gO.SetActive(true);
        index = (index + 1) % size;
        
        return gO;
    }

}
