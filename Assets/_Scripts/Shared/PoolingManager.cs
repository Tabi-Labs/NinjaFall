using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingManager<T> : MonoBehaviour where T : Component
{
    private int size = 15;
    List<GameObject> pool;

    private int index = 0;

    public void InitializePool(T element, Transform parent, int size = 15, string name = "PooledObject_")
    {
        pool = new List<GameObject>();
        this.size = size;


        for (int i = 0; i < this.size; i++) {
            GameObject gO = new GameObject(name + i); // Creamos un GameObject
            gO.AddComponent<T>();  // Añadimos el componente de tipo T
            gO.transform.parent = parent;  // Establecemos el padre


            // Desactivamos el GameObject (para que no esté activo al principio)
            gO.SetActive(false);

            //pool.Add(component);

            pool.Add(gO);
            //Instantiate(gO, parent);
        }
    }

    public GameObject GetFree() { 
        GameObject gO = pool[index];
        gO.SetActive(true);
        index = index + 1 >= size ? 0 : index++;
        return gO;
    }

    public List<GameObject> GetPools() {
        return pool;
    }
}
