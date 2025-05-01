using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem;

public class BotBrain : MonoBehaviour
{
    public int botId;
    private BotController botController;
    public float decisionInterval = 0.5f; // Intervalo para tomar decisiones
    public float wallCheckInterval = 0.25f; // Intervalo para tomar decisiones
    public float obstacleCheckDistance = 1.5f; // Distancia para revisar obstaculos
    public float targetDistanceThreshold = 1.0f; // Distancia para perseguir al jugador

    private GameObject closestPlayer;

    void Start()
    {
        if (botId <= 0)
        {
            Debug.LogError("BotId is not assigned. Please assign a valid BotId in the inspector.");
            enabled = false;
            return;
        }

        botController = BotManager.Instance.GetBotById(botId);
        
        if (botController == null)
        {
            Debug.LogError("BotController not found for BotId: " + botId);
            enabled = false;
            return;
        }
        
        StartCoroutine(MakeDecisions());
        StartCoroutine(CheckWallsAndJump());
    }

    private IEnumerator MakeDecisions()
    {
        while (true)
        {
            botController.ResetInputs(); // Resetear entradas antes de tomar decisiones
            FindClosestPlayer();
            if (closestPlayer != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, closestPlayer.transform.position);

                MoveTowardsTarget(closestPlayer.transform.position);
                if (distanceToPlayer <= targetDistanceThreshold)
                {
                    Attack();
                }
            }
            else
            {
                // No hay jugadores cercanos, realizar una accion por defecto
                //WanderAround();
            }
            yield return new WaitForSeconds(decisionInterval);
        }
    }

    private IEnumerator CheckWallsAndJump()
    {
        while (true)
        {
            if(ObstacleIsAhead())
            {
                Jump();
            }
            yield return new WaitForSeconds(wallCheckInterval);
        }
    }

    private void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player").Where(player => player.GetComponent<PlayerInput>() != null).ToArray();
        players = players.Where(player => player != gameObject).ToArray();

        if (players.Length == 0)
        {
            closestPlayer = null;
            return;
        }

        closestPlayer = players.OrderBy(player => Vector3.Distance(transform.position, player.transform.position)).FirstOrDefault();
    }

    private bool ObstacleIsAhead()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, obstacleCheckDistance, LayerMask.GetMask("Obstacles"));
        Debug.DrawRay(transform.position, transform.right * obstacleCheckDistance, Color.red);
        if (hit.collider)
        {
            Debug.Log("Obstacle detected: " + hit.collider.gameObject.name);
            return true;
        }
        return false;
    }

    // Actions
    // --------------------------------------------------------------------------------
    
    private void Attack()
    {
        _ = botController.PressAndReleaseButton(GamepadButton.West, 0.1f);
    }

    private void Jump()
    {
        _ = botController.PressAndReleaseButton(GamepadButton.South, 0.1f);
    }

    private void Dash()
    {
        _ = botController.PressAndReleaseButton(GamepadButton.East, 0.1f);
    }


    private void MoveTowardsTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        Vector2 targetDir = new Vector2(direction.x, direction.z);
        botController.MoveJoystick(targetDir, "leftStick");
    }

    private void ThrowShurikenToTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        Vector2 targetDir = new Vector2(direction.x, direction.z);
        botController.MoveJoystick(Vector2.zero, "rightStick");
        _ = botController.PressAndReleaseButton(GamepadButton.East, 0.5f);
        
    }
}