using System;
using Cinemachine;
using UnityEngine;

public class CameraScript : MonoBehaviour {
    public static CameraScript I;
    [SerializeField] private CinemachineVirtualCamera carCamera;
    [SerializeField] private CinemachineVirtualCamera frogCamera;
    [SerializeField] private CinemachineVirtualCamera goalCamera;
    [SerializeField] private CinemachineVirtualCamera carZoomCamera;

    private void Awake() {
        if (I && I != this) {
            Destroy(I.gameObject);
        }

        I = this;
    }

    public enum TargetCam {
        Car,
        Frog,
        Goal,
        CarZoom
    }

    public static void SwitchCameras(TargetCam cam) {
        I.carCamera.Priority = 1;
        I.frogCamera.Priority = 1;
        I.goalCamera.Priority = 1;
        I.carZoomCamera.Priority = 1;

        CinemachineVirtualCamera newActiveCam = cam switch {
            TargetCam.Car => I.carCamera,
            TargetCam.Frog => I.frogCamera,
            TargetCam.Goal => I.goalCamera,
            TargetCam.CarZoom => I.carZoomCamera,
            _ => throw new ArgumentOutOfRangeException($"no case for {cam}")
        };
        newActiveCam.Priority = 2;
    }

    public static void AssignFrogTarget(Transform frog) {
        I.frogCamera.Follow = frog;
        I.frogCamera.LookAt = frog;
    }

    public static void AssignGoalTarget(Transform goal) {
        I.goalCamera.Follow = goal;
        I.goalCamera.LookAt = goal;
    }
    
    public static void AssignCarZoomTarget(Transform car) {
        I.carZoomCamera.Follow = car;
        I.carZoomCamera.LookAt = car;
    }
}
