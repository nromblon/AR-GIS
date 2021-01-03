using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore.Examples.ObjectManipulation;

/// <summary>
/// A variant of ExtendedRotationManipulator (Which is a variant of ARCore's RotationManipulator)
/// but has the CityManager.HasManipulationPerformed flag sets
/// </summary>
public class CityRotationManipulator : RotationManipulator {

	private CityManager cityManager;

	public bool CanOperateWithDrag = false;

	private void Start() {
		cityManager = GetComponent<CityManager>();
	}

	protected override bool CanStartManipulationForGesture(DragGesture gesture) {
		if (!CanOperateWithDrag)
			return false;

		if (!ARSceneController.Instance.AllowManipulation)
			return false;

		return base.CanStartManipulationForGesture(gesture);
	}
	/// <summary>
	/// Returns true if the manipulation can be started for the given Twist gesture.
	/// </summary>
	/// <param name="gesture">The current gesture.</param>
	/// <returns>True if the manipulation can be started.</returns>
	protected override bool CanStartManipulationForGesture(TwistGesture gesture) {
		if (!IsSelected()) {
			return false;
		}

		if (!ARSceneController.Instance.AllowManipulation)
			return false;
		
		return true;
	}

	protected override void OnStartManipulation(DragGesture gesture) {
		cityManager.IsManipulating = true;
		base.OnStartManipulation(gesture);
	}

	protected override void OnStartManipulation(TwistGesture gesture) {
		cityManager.IsManipulating = true;
		base.OnStartManipulation(gesture);
	}

	protected override void OnEndManipulation(DragGesture gesture) {
		base.OnEndManipulation(gesture);
		cityManager.IsManipulating = false;
		cityManager.HasManipulationPerformed = true;
	}

	protected override void OnEndManipulation(TwistGesture gesture) {
		base.OnEndManipulation(gesture);
		cityManager.IsManipulating = false;
		cityManager.HasManipulationPerformed = true;
	}
}
