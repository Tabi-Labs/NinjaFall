using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Sword : MonoBehaviour
{

    public float knockbackForce = 5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Detectar colisiones con otros objetos
    void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si se golpea a otro jugador
        if (other.CompareTag("Player"))
        {
            Debug.Log("Golpe a otro jugador");

            // Retroceso del jugador golpeado
            Rigidbody2D playerRigidbody = other.transform.root.GetComponentInParent<Rigidbody2D>();
            if (playerRigidbody != null)
            {
                Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                playerRigidbody.AddForce(knockbackDirection * knockbackForce * 10, ForceMode2D.Impulse);
            }
        }

        // Verificar si se choca con otra espada
        if (other.CompareTag("Sword"))
        {
            Debug.Log("Choque de espadas");

            // Retroceso del jugador propio
            Rigidbody2D myRigidbody = transform.root.GetComponentInParent<Rigidbody2D>();
            if (myRigidbody != null)
            {
                Vector2 knockbackDirection = (myRigidbody.transform.position - other.transform.position).normalized;
                myRigidbody.AddForce(knockbackDirection * knockbackForce * 100, ForceMode2D.Impulse);
            }
        }
    }
}
