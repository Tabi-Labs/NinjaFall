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

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Destruye el proyectil al colisionar
        Destroy(gameObject);
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(gameObject);
    }
}
