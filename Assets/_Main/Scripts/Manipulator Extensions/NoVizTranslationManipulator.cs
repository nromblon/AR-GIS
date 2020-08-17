﻿using UnityEngine;
using GoogleARCore;
using GoogleARCore.Examples.ObjectManipulationInternal;
using GoogleARCore.Examples.ObjectManipulation;

/// <summary>
/// Exactly the same as TranslationManipulator, but without the visualization requirement
/// </summary>
public class NoVizTranslationManipulator : Manipulator
{
	/// <summary>
	/// The translation mode of this object.
	/// </summary>
	public TransformationUtility.TranslationMode ObjectTranslationMode;

	/// <summary>
	/// The maximum translation distance of this object.
	/// </summary>
	public float MaxTranslationDistance;

	private const float k_PositionSpeed = 12.0f;
	private const float k_DiffThreshold = 0.0001f;

	private bool m_IsActive = false;
	private Vector3 m_DesiredAnchorPosition;
	private Vector3 m_DesiredLocalPosition;
	private Quaternion m_DesiredRotation;
	private float m_GroundingPlaneHeight;
	private TrackableHit m_LastHit;

	/// <summary>
	/// The Unity's Start method.
	/// </summary>
	protected void Start() {
		m_DesiredLocalPosition = new Vector3(0, 0, 0);
	}

	/// <summary>
	/// The Unity's Update method.
	/// </summary>
	protected override void Update() {
		base.Update();
		UpdatePosition();
	}

	/// <summary>
	/// Returns true if the manipulation can be started for the given gesture.
	/// </summary>
	/// <param name="gesture">The current gesture.</param>
	/// <returns>True if the manipulation can be started.</returns>
	protected override bool CanStartManipulationForGesture(DragGesture gesture) {
		if (gesture.TargetObject == null) {
			return false;
		}

		// If the gesture isn't targeting this item, don't start manipulating.
		if (gesture.TargetObject != gameObject) {
			return false;
		}

		// Select it.
		Select();

		return true;
	}

	/// <summary>
	/// Function called when the manipulation is started.
	/// </summary>
	/// <param name="gesture">The current gesture.</param>
	protected override void OnStartManipulation(DragGesture gesture) {
		m_GroundingPlaneHeight = transform.parent.position.y;
	}

	/// <summary>
	/// Continues the translation.
	/// </summary>
	/// <param name="gesture">The current gesture.</param>
	protected override void OnContinueManipulation(DragGesture gesture) {
		m_IsActive = true;

		TransformationUtility.Placement desiredPlacement =
			TransformationUtility.GetBestPlacementPosition(
				transform.parent.position, gesture.Position, m_GroundingPlaneHeight, 0.03f,
				MaxTranslationDistance, ObjectTranslationMode);

		if (desiredPlacement.HoveringPosition.HasValue &&
			desiredPlacement.PlacementPosition.HasValue) {
			// If desired position is lower than current position, don't drop it until it's
			// finished.
			m_DesiredLocalPosition = transform.parent.InverseTransformPoint(
				desiredPlacement.HoveringPosition.Value);

			m_DesiredAnchorPosition = desiredPlacement.PlacementPosition.Value;

			m_GroundingPlaneHeight = desiredPlacement.UpdatedGroundingPlaneHeight;

			if (desiredPlacement.PlacementRotation.HasValue) {
				// Rotate if the plane direction has changed.
				if (((desiredPlacement.PlacementRotation.Value * Vector3.up) - transform.up)
					.magnitude > k_DiffThreshold) {
					m_DesiredRotation = desiredPlacement.PlacementRotation.Value;
				}
				else {
					m_DesiredRotation = transform.rotation;
				}
			}

			if (desiredPlacement.PlacementPlane.HasValue) {
				m_LastHit = desiredPlacement.PlacementPlane.Value;
			}
		}
	}

	/// <summary>
	/// Finishes the translation.
	/// </summary>
	/// <param name="gesture">The current gesture.</param>
	protected override void OnEndManipulation(DragGesture gesture) {
		GameObject oldAnchor = transform.parent.gameObject;

		Pose desiredPose = new Pose(m_DesiredAnchorPosition, m_LastHit.Pose.rotation);

		Vector3 desiredLocalPosition =
			transform.parent.InverseTransformPoint(desiredPose.position);

		if (desiredLocalPosition.magnitude > MaxTranslationDistance) {
			desiredLocalPosition = desiredLocalPosition.normalized * MaxTranslationDistance;
		}

		desiredPose.position = transform.parent.TransformPoint(desiredLocalPosition);

		Anchor newAnchor = m_LastHit.Trackable.CreateAnchor(desiredPose);
		transform.parent = newAnchor.transform;

		Destroy(oldAnchor);

		m_DesiredLocalPosition = Vector3.zero;

		// Rotate if the plane direction has changed.
		if (((desiredPose.rotation * Vector3.up) - transform.up).magnitude > k_DiffThreshold) {
			m_DesiredRotation = desiredPose.rotation;
		}
		else {
			m_DesiredRotation = transform.rotation;
		}

		// Make sure position is updated one last time.
		m_IsActive = true;
	}

	private void UpdatePosition() {
		if (!m_IsActive) {
			return;
		}

		// Lerp position.
		Vector3 oldLocalPosition = transform.localPosition;
		Vector3 newLocalPosition = Vector3.Lerp(
			oldLocalPosition, m_DesiredLocalPosition, Time.deltaTime * k_PositionSpeed);

		float diffLenght = (m_DesiredLocalPosition - newLocalPosition).magnitude;
		if (diffLenght < k_DiffThreshold) {
			newLocalPosition = m_DesiredLocalPosition;
			m_IsActive = false;
		}

		transform.localPosition = newLocalPosition;

		// Lerp rotation.
		Quaternion oldRotation = transform.rotation;
		Quaternion newRotation =
			Quaternion.Lerp(oldRotation, m_DesiredRotation, Time.deltaTime * k_PositionSpeed);
		transform.rotation = newRotation;

		// Avoid placing the selection higher than the object if the anchor is higher than the
		// object.
		float newElevation =
			Mathf.Max(0, -transform.InverseTransformPoint(m_DesiredAnchorPosition).y);
	}
}

