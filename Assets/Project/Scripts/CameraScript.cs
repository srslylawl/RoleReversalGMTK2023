using Cinemachine;
using UnityEngine;

public class CameraScript : MonoBehaviour {
    public static CameraScript I;
    [SerializeField] private CinemachineVirtualCamera carCamera;
    [SerializeField] private CinemachineVirtualCamera frogCamera;

    private bool carCamActive = true;


    private void Awake() {
        if (I && I != this) {
            Destroy(I.gameObject);
        }

        I = this;
    }

    public static void SwitchCameras(bool toCar) {
        var newActiveCam = toCar ? I.carCamera : I.frogCamera;
        var otherCam = toCar ? I.frogCamera : I.carCamera;

        newActiveCam.Priority = 2;
        otherCam.Priority = 1;
        I.carCamActive = toCar;
    }

    public static void AssignFrogTarget(Transform frog) {
        I.frogCamera.Follow = frog;
        I.frogCamera.LookAt = frog;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.LeftAlt)) {
            SwitchCameras(!carCamActive);
        }
    }
}
