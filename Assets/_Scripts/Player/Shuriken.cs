using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shuriken : MonoBehaviour
{

    public float speed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Movimiento del shuriken
        transform.Translate(Vector2.right * speed * Time.deltaTime);

    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // TODO Destruir
            Player player = other.transform.root.GetComponent<Player>();
            if (player != null)
            {
                Debug.Log("Player eliminado por shuriken");
                player.WasHitted(); // Llamar a la función de destrucción
            }
        }

        Destroy(gameObject);
    }
}
