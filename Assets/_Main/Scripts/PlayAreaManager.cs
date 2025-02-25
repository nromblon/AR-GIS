﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using GoogleARCore;
using GoogleARCore.Examples.ObjectManipulation;

public class PlayAreaManager : Manipulator
{
	private static PlayAreaManager instance;
	public static PlayAreaManager Instance {
		get { return instance; }
	}

	public GameObject PlayAreaPrefab;
	private PlayAreaController playArea;
	public PlayAreaController PlayArea {
		get { return playArea; }
	}
	public bool hasPlacedPlayArea;
	public bool hasConfirmedPlayArea;


	[Header("UI Setup")]
	public ConfirmBtnController confirmBtn;

	private ARSceneController sceneController;

	private void Awake() {
		instance = this;
	}

	// Start is called before the first frame update
	void Start()
    {
		hasPlacedPlayArea = false;
		hasConfirmedPlayArea = false;
		sceneController = ARSceneController.Instance;
		this.Select();
    }

	#region Manipulator Methods
	protected override bool CanStartManipulationForGesture(TapGesture gesture) {
		#region Input pre-checks
		// Should not handle input if the player is pointing on UI.
		if (EventSystem.current.IsPointerOverGameObject(gesture.FingerId)) {
			return false;
		}
		#endregion
		
		// Only accept when no object is selected, to start dropping bounds
		if (gesture.TargetObject != null)
			return false;
		
		if (hasPlacedPlayArea)
			return false;
		
		if (!IsSelected())
			return false;

		return true;
	}

	protected override void OnEndManipulation(TapGesture gesture) {
		if (gesture.WasCancelled)
			return;

		if (gesture.TargetObject != null)
			return;

		// Raycast against the location the player touched to search for planes.
		TrackableHit hit;
		TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon;

		if (Frame.Raycast(
			gesture.StartPosition.x, gesture.StartPosition.y, raycastFilter, out hit)) {
			// Use hit pose and camera pose to check if hittest is from the
			// back of the plane, if it is, no need to create the anchor.
			if ((hit.Trackable is DetectedPlane) &&
				Vector3.Dot(Camera.main.transform.position - hit.Pose.position,
					hit.Pose.rotation * Vector3.up) < 0) {
			}
			else {
				// Instantiate game object at the hit pose.
				playArea = Instantiate(PlayAreaPrefab, hit.Pose.position, hit.Pose.rotation)
					.GetComponent<PlayAreaController>();

				// Create an anchor to allow ARCore to track the hitpoint as understanding of
				// the physical world evolves.
				var anchor = hit.Trackable.CreateAnchor(hit.Pose);

				this.Deselect();

				hasPlacedPlayArea = true;

				// Make manipulator a child of the anchor.
				playArea.transform.parent = anchor.transform;

				playArea.Select();

				// Show Confirm Button
				confirmBtn.ShowButton(true);
			}
		}
	}
	#endregion

	public void ConfirmPlacement() {
		hasConfirmedPlayArea = true;

		Bounds PABounds = playArea.Bounds;
		sceneController.OnPlayAreaConfirmed(PABounds,this);
		playArea.OnPlacementConfirm();
	}

	public void RemovePlacement() {
		CityManager.Instance.OnCityRemoved();

		hasPlacedPlayArea = false;
		hasConfirmedPlayArea = false;

		if(playArea != null) {
			// Destroy the anchor
			Destroy(playArea.transform.parent.gameObject);
		}
		playArea = null;

		Select();
	}
}

