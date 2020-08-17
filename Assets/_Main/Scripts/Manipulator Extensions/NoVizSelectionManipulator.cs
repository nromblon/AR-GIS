using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore.Examples.ObjectManipulation;
using GoogleARCore;


/// <summary>
/// Exactly the same as manipulator except selection visualization is not required
/// </summary>
public class NoVizSelectionManipulator : Manipulator {

	private float m_ScaledElevation;

	/// <summary>
	/// Should be called when the object elevation changes, to make sure that the Selection
	/// Visualization remains always at the plane level. This is the elevation that the object
	/// has, independently of the scale.
	/// </summary>
	/// <param name="elevation">The current object's elevation.</param>
	public void OnElevationChanged(float elevation) {
		m_ScaledElevation = elevation * transform.localScale.y;
	}

	/// <summary>
	/// Should be called when the object elevation changes, to make sure that the Selection
	/// Visualization remains always at the plane level. This is the elevation that the object
	/// has multiplied by the local scale in the y coordinate.
	/// </summary>
	/// <param name="scaledElevation">The current object's elevation scaled with the local y
	/// scale.</param>
	public void OnElevationChangedScaled(float scaledElevation) {
		m_ScaledElevation = scaledElevation;
	}

	/// <summary>
	/// The Unity Update() method.
	/// </summary>
	protected override void Update() {
		base.Update();
		if (transform.hasChanged) {
			float height = -m_ScaledElevation / transform.localScale.y;
		}
	}

	/// <summary>
	/// Returns true if the manipulation can be started for the given gesture.
	/// </summary>
	/// <param name="gesture">The current gesture.</param>
	/// <returns>True if the manipulation can be started.</returns>
	protected override bool CanStartManipulationForGesture(TapGesture gesture) {
		return true;
	}

	/// <summary>
	/// Function called when the manipulation is ended.
	/// </summary>
	/// <param name="gesture">The current gesture.</param>
	protected override void OnEndManipulation(TapGesture gesture) {
		if (gesture.WasCancelled) {
			return;
		}

		if (ManipulationSystem.Instance == null) {
			return;
		}

		GameObject target = gesture.TargetObject;
		if (target == gameObject) {
			Select();
		}

		// Raycast against the location the player touched to search for planes.
		TrackableHit hit;
		TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon;

		if (!Frame.Raycast(
			gesture.StartPosition.x, gesture.StartPosition.y, raycastFilter, out hit)) {
			Deselect();
		}
	}
}
