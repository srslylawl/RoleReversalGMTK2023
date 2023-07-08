using Cinemachine;
using UnityEngine;

public class CameraScript : MonoBehaviour {
    [SerializeField] private CinemachineVirtualCamera carCamera;
    [SerializeField] private CinemachineVirtualCamera frogCamera;

    private bool carCamActive = true;

    public void SwitchCameras(bool toCar) {
        var newActiveCam = toCar ? carCamera : frogCamera;
        var otherCam = toCar ? frogCamera : carCamera;

        newActiveCam.Priority = 2;
        otherCam.Priority = 1;
        carCamActive = toCar;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.LeftAlt)) {
            SwitchCameras(!carCamActive);
        }
    }
}
