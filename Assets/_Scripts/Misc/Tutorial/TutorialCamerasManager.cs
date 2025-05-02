using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class TutorialCamerasManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera[] cameras;
    [SerializeField] private int currentCameraIndex = 0;

    private void Awake()
    {
        if (cameras.Length == 0) return;

        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(i == currentCameraIndex);
        }
    }
    public void SwitchToNextCamera()
    {
        if (cameras.Length == 0) return;

        cameras[currentCameraIndex].gameObject.SetActive(false);
        currentCameraIndex = (currentCameraIndex + 1) % cameras.Length;
        cameras[currentCameraIndex].gameObject.SetActive(true);
    }

    public void SwitchToPreviousCamera()
    {
        if (cameras.Length == 0) return;

        cameras[currentCameraIndex].gameObject.SetActive(false);
        currentCameraIndex = (currentCameraIndex - 1 + cameras.Length) % cameras.Length;
        cameras[currentCameraIndex].gameObject.SetActive(true);
    }
}
