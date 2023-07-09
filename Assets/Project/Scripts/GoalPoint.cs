using System;
using UnityEngine;

public class GoalPoint : MonoBehaviour {
	[SerializeField] private Mode mode;
	[SerializeField] private GameObject Pond;
	[SerializeField] private GameObject PondIndicator;
	[SerializeField] private GameObject Car;
	[SerializeField] private GameObject CarIndicator;

	private enum Mode {
		Frog,
		Car
	}

	public Action<IController> OnGoalReached;

	public void SetGoalActive(bool active) {
		if (mode == Mode.Frog) {
			PondIndicator.SetActive(active);
		}

		if (mode == Mode.Car) {
			CarIndicator.SetActive(active);
		}
	}

#if UNITY_EDITOR

	private void OnValidate() {
		if (mode == Mode.Car) {
			SetModeToCar();
		}
		else {
			SetModeToFrog();
		}
	}

#endif

	private void SetModeToFrog() {
		Pond.SetActive(true);
		Car.SetActive(false);
	}

	private void SetModeToCar() {
		Pond.SetActive(false);
		Car.SetActive(true);
	}

	private void OnTriggerEnter(Collider other) {
		var ctr = other.GetComponentInParent<IController>();

		if (ctr == null) {
			return;
		}

		if (mode == Mode.Frog) {
			//Don't allow cars to use ponds as goals
			if (ctr is CarController) {
				//is car, don't call back!
				return;
			}
		}

		if (mode == Mode.Car) {
			if (ctr is FrogController) {
				return;
			}
		}

		OnGoalReached?.Invoke(ctr);
	}
}