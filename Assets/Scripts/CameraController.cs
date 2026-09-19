
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Camera References")]
    public CinemachineCamera gameCam; 
    public CinemachineCamera winCam;  

    private bool isShowingWideView = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleCamera();
        }
    }

    public void ToggleCamera()
    {
        isShowingWideView = !isShowingWideView;

        if (isShowingWideView)
        {
            gameCam.Priority = 5;
            winCam.Priority = 10;
        }
        else
        {
            gameCam.Priority = 10;
            winCam.Priority = 5;
        }
    }
}