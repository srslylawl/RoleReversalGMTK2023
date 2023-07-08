using UnityEngine;

public class InputManager : MonoBehaviour {
	private Camera cam;

	private Vector3 mouseWorldPos;


	private CarController CarController;
    public FrogController FrogController;

	public bool UpdateControllers;

    private void Awake() {
		cam = Camera.main;
	}

	public void SetCarController(CarController controller) {
		if (CarController != null) {
			CarController.ReceiveInputHeld(false);
		}

		CarController = controller;
	}
	
	public void SetFrogController(FrogController controller) {
		if (FrogController != null) {
			FrogController.ReceiveInputHeld(false);
		}

		FrogController = controller;
	}


	private void Update() {
		if (!UpdateControllers) {
			return;
		}
		var screenPos = Input.mousePosition;
		var ray = cam.ScreenPointToRay(screenPos);
		// Define the plane
		Vector3 planeNormal = Vector3.up; // Plane normal pointing upwards
		Vector3 planePoint = new Vector3(0f, 0f, 0f); // Point on the plane
        
		// Calculate the intersection
		float t = Vector3.Dot(planePoint - ray.origin, planeNormal) / Vector3.Dot(ray.direction, planeNormal);
		Vector3 intersectionPoint = ray.origin + t * ray.direction;

		mouseWorldPos = intersectionPoint;

		var inputHeld = Input.GetMouseButton(0);
		var inputUp = Input.GetMouseButtonUp(0);
		
		CarController?.ReceiveTargetInput(mouseWorldPos);
		CarController?.ReceiveInputHeld(inputHeld);
        FrogController?.ReceiveTargetInput(mouseWorldPos);
		FrogController?.ReceiveInputHeld(inputHeld);
		
		if (inputUp) FrogController?.ReceiveInputUp(true);
    }

	private void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(mouseWorldPos, .3f);
	}
}
