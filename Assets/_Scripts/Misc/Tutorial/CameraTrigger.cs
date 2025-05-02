using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    [SerializeField] private GameEvent nextRoomEvent;
    [SerializeField] private GameEvent previousRoomEvent;
    Vector3 _centerPos;
    private bool _isInTrigger = false;
    private RoomDirection _originRoom = RoomDirection.LEFT;
    private RoomDirection _targetRoom = RoomDirection.RIGHT;
    
    #region ---- UNITY CALLBACKS ----
    private void Awake()
    {
        _centerPos = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        _isInTrigger = true; // Control variable because the player has 3 colliders. With this check it only triggers once.
        CheckPlayerDirection(other, ref _originRoom);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(!_isInTrigger) return;
        if (other.CompareTag("Player"))
        {
            _isInTrigger = false;
            CheckPlayerDirection(other, ref _targetRoom);
            RaiseCameraEvent();
        }
    }
    #endregion

    #region ---- PRIVATE FUNCTIONS ----
    private void CheckPlayerDirection(Collider2D other, ref RoomDirection room)
    {
        var playerPos = other.transform.position;
        var offset = playerPos.x - _centerPos.x;
        if(offset < 0f)
        {   
            room = RoomDirection.LEFT;
        }
        else
        {
            room = RoomDirection.RIGHT;
        }
    }

    private void RaiseCameraEvent()
    {
         if(_originRoom == RoomDirection.LEFT && _targetRoom == RoomDirection.RIGHT)
            {
                nextRoomEvent.Raise(null,null);
            }
            else if(_originRoom == RoomDirection.RIGHT && _targetRoom == RoomDirection.LEFT)
            {
                previousRoomEvent.Raise(null,null);
            }
    }
    #endregion
}

public enum RoomDirection
{
    LEFT,
    RIGHT
}
