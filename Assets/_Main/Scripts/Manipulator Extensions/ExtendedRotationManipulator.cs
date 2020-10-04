using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore.Examples.ObjectManipulation;

public class ExtendedRotationManipulator : RotationManipulator {

	public bool CanOperateWithDrag = false;

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
}
