using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using GoogleARCore;
using GoogleARCore.CrossPlatform;
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

	private AsyncTask<CloudAnchorResult> cloudAnchorTask;

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

	private void Update() {
		// Handle Cloud Anchor Resolution here (If Device is not a Host)
		if (sceneController.applicationMode != ApplicationMode.Client)
			return;

		
		//if (sceneController.applicationMode == ApplicationMode.Client) {
		//	AsyncTask<CloudAnchorResult> asynCtask = XPSession.ResolveCloudAnchor(sceneController.AnchorId);
		//}
		// else { 
		//var pose = Frame.Pose;
		//var mapQuality = XPSession.EstimateFeatureMapQualityForHosting(pose); // only added in SDK ver 1.20. currently on 1.18

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

		if (sceneController.applicationMode == ApplicationMode.Client)
			return false;

		return true;
	}

	protected override void OnEndManipulation(TapGesture gesture) {
		if (gesture.WasCancelled)
			return;

		if (gesture.TargetObject != null)
			return;

		// Cloud Anchor Hosting is handled here. (If Device is the Host)

		// Raycast against the location the player touched to search for planes.
		TrackableHit hit;
		TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon;

		if (Frame.Raycast(
			gesture.StartPosition.x, gesture.StartPosition.y, raycastFilter, out hit)) {

			var dotProduct = Vector3.Dot(Camera.main.transform.position - hit.Pose.position, hit.Pose.rotation * Vector3.up);
			// Comnpares hit pose and camera pose to check if TrackableHit is from the
			// back of the plane. If it is, Do not create an anchor.
			if ((hit.Trackable is DetectedPlane) && !(dotProduct < 0)) {
				// Instantiate game object at the hit pose.
				playArea = Instantiate(PlayAreaPrefab, hit.Pose.position, hit.Pose.rotation)
					.GetComponent<PlayAreaController>();

				// Create an anchor to allow ARCore to track the hitpoint as understanding of
				// the physical world evolves.
				var anchor = hit.Trackable.CreateAnchor(hit.Pose);

				// Host anchor
				cloudAnchorTask = XPSession.CreateCloudAnchor(anchor);
				//if (sceneController.applicationMode == ApplicationMode.Host) {
				//	// place if statement before raycast
				//	AsyncTask<CloudAnchorResult> asyncTask = XPSession.CreateCloudAnchor(anchor);
				//}

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

	IEnumerator WaitForCloudAnchorTask(System.Action<XPAnchor> callback) {
		while (!cloudAnchorTask.IsComplete) {
			yield return null;
		}

		if(cloudAnchorTask.Result.Response == CloudServiceResponse.Success) {
			callback(cloudAnchorTask.Result.Anchor);
		}
		else {
			Debug.LogError($"[PlayAreaManager] Hosting/Resolution Failed! Cloud Service Response: " +
				$"{cloudAnchorTask.Result.Response.ToString()}");
		}
		
	}

	private void OnCloudAnchorHostSuccess(XPAnchor result) {
		sceneController.SetCloudAnchorId(result.CloudId);
		// Re-parent play area to XPAnchor?
	}

	private void OnCloudAnchorResolveSuccess(XPAnchor result) {
		// How to instantiate anchor? Or is it automatic? (Its automatic: though a different object is returned in the form 
		// of XPAnchor)
		// Instantiate Play Area Object on Resolved Anchor Object
		var newParent = result.transform;
	}
}

