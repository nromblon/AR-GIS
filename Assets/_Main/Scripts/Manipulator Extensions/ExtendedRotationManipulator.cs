using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore.Examples.ObjectManipulation;

public class ExtendedRotationManipulator : RotationManipulator {
	/// <summary>
	/// Returns true if the manipulation can be started for the given Twist gesture.
	/// </summary>
	/// <param name="gesture">The current gesture.</param>
	/// <returns>True if the manipulation can be started.</returns>
	protected override bool CanStartManipulationForGesture(TwistGesture gesture) {
		if (!IsSelected()) {
			return false;
		}

		return true;
	}
}
