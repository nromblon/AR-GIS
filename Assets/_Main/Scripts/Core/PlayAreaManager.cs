using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using GoogleARCore;
using GoogleARCore.CrossPlatform;
using GoogleARCore.Examples.ObjectManipulation;
using GoogleARCore.Examples.CloudAnchors;
using Mirror;
using FixCityAR;

public class PlayAreaManager : Manipulator
{
	private static PlayAreaManager instance;
	public static PlayAreaManager Instance {
		get { return instance; }
	}

	private PlayAreaController playArea;
	public PlayAreaController PlayArea {
		get { return playArea; }
	}

	private ARSceneController ARSceneCtrl;

	[Header("Setup")]
	[SerializeField] private ConfirmBtnController confirmBtn;
	[SerializeField] private ConfirmBtnController tryHostBtn;

	[SerializeField] private GameObject playAreaPrefab;
	[SerializeField] private GameObject XPAnchorVisualizer;
	[SerializeField] private ARCoreWorldOriginHelper worldOriginHelper;

	[Header("Runtime Data")]
	public bool hasPlacedPlayArea;
	public bool hasConfirmedPlayArea;
	
	public ARUser localUser;
	private XPAnchor cloudAnchor;
	AsyncTask<CloudAnchorResult> anchorTask;

	private void Awake() {
		instance = this;

		hasPlacedPlayArea = false;
		hasConfirmedPlayArea = false;
		ARSceneCtrl = ARSceneController.Instance;
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

		if (ARSceneCtrl.applicationMode == ApplicationMode.Client)
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

			// Comnpares hit pose and camera pose to check if TrackableHit is from the
			// back of the plane. If it is, Do not create an anchor.
			var dotProduct = Vector3.Dot(Camera.main.transform.position - hit.Pose.position, hit.Pose.rotation * Vector3.up);
			if ((hit.Trackable is DetectedPlane) && !(dotProduct < 0)) {

				// Instantiate game object at the hit pose.
				playArea = Instantiate(playAreaPrefab, hit.Pose.position, hit.Pose.rotation)
					.GetComponent<PlayAreaController>();

				//playAreaPrefab.SetActive(true);
				//playAreaPrefab.transform.SetPositionAndRotation(hit.Pose.position, hit.Pose.rotation);
				//playArea = playAreaPrefab.GetComponent<PlayAreaController>();
				
				// Create an anchor to allow ARCore to track the hitpoint as understanding of
				// the physical world evolves.
				var anchor = hit.Trackable.CreateAnchor(hit.Pose);

				// Attach PlayArea Object as a child of the anchor.
				playArea.transform.parent = anchor.transform;

				this.Deselect();
				playArea.Select();

				// Show Confirm Button
				confirmBtn.ShowButton(true);
				
				hasPlacedPlayArea = true;
			}
		}
	}
	#endregion

	/// <summary>
	/// Finalizes the position, rotation, and scale of the play area.
	/// This Function should only occur on the Host's device.
	/// </summary>
	public void ConfirmPlacement() {
		Debug.Log($"[{this.GetType().Name}] ConfirmPlacement() called.");
		hasConfirmedPlayArea = true;

		Bounds PABounds = playArea.Bounds;
		ARSceneCtrl.OnPlayAreaConfirmed(PABounds,this);

		// Notify Play Area anim controller
		playArea.OnPlacementConfirm();

		if (((NewNetworkManager)NewNetworkManager.singleton).isOnline)
			TryHostAnchor();
	}

	/// <summary>
	/// Called by non-host. Creates a Copy of the server's play area into the client's scene.
	/// </summary>
	public void ClientCreatePlayArea(TfValues paTf, TfValues cTf, Vector3 cityContainerScale) {
		var paGO = Instantiate(playAreaPrefab, cloudAnchor.transform);
		//Adjust Play Area Transform values.
		paGO.transform.localPosition = paTf.localPosition;
		paGO.transform.localRotation = paTf.localRotation;
		paGO.transform.localScale = paTf.localScale;

		playArea = paGO.GetComponent<PlayAreaController>();
		playArea.CityContainer.transform.localScale = cityContainerScale;

		// Attach city inside.
		ARSceneCtrl.OnPlayAreaConfirmed(cTf, this);

		hasPlacedPlayArea = true;
		hasConfirmedPlayArea = true;

		// Notify Play Area anim controller
		playArea.OnPlacementConfirm();
	}

	/// <summary>
	/// Removes the placement of the play area.
	/// This function should only be called by the host's device.
	/// </summary>
	public void RemovePlacement() {
		CityManager.Instance.DetachCity();

		// Destroy XP Anchor
		if (cloudAnchor != null) {
			Destroy(cloudAnchor.gameObject);
		}

		//if(playArea != null) {
		//	// Destroy the anchor
		//	Destroy(playArea.transform.parent.gameObject);
		//}

		// Destroy play area
		Destroy(playArea.gameObject);

		playArea = null;
		hasPlacedPlayArea = false;
		hasConfirmedPlayArea = false;
		localUser.CityTfBehaviour.OnPlayAreaRemoved();
		ARSceneCtrl.SetCloudAnchorId("");

		Select();
	}

	/// <summary>
	/// Called by non-host. Detaches the city component from the play area and destroys the play area object.
	/// </summary>
	public void ClientRemovePlayArea() {
		CityManager.Instance.DetachCity();
		ARSceneCtrl.SetCloudAnchorId("");
		Destroy(this.cloudAnchor);
		playArea = null;
		hasPlacedPlayArea = false;
		hasConfirmedPlayArea = false;
	}

	public void TryHostAnchor() {
		// Attempt to Host Anchor
		var anchor = playArea.GetComponentInParent<Anchor>();

		Debug.Log($"[{this.GetType().Name}] Attempting to host anchor");

		ARSceneCtrl._ShowAndroidToastMessage("Attempting to Host...");
		anchorTask = XPSession.CreateCloudAnchor(anchor).ThenAction(OnHostComplete);
	}

	/// <summary>
	/// Callback for when the XPSession.CreateCloudAnchor is a success. Creates a new Anchor, of type XPAnchor, as well as its
	/// Id (CloudId) in the ARCore Cloud Anchor service. Anchor is null if Hosting failed.
	/// </summary>
	/// <param name="result"></param>
	private void OnHostComplete(CloudAnchorResult result) {
		if (result.Response == CloudServiceResponse.Success) {
			// For debugging only.
			//Instantiate(XPAnchorVisualizer, result.Anchor.transform);

			Debug.Log($"[{this.GetType().Name}] AnchorHost Success! CloudId: {result.Anchor.CloudId}");
			ARSceneCtrl._ShowAndroidToastMessage("Anchor Host Success!");
			this.cloudAnchor = result.Anchor;
			ARSceneCtrl.SetCloudAnchorId(cloudAnchor.CloudId);
			
			// Set world Origin to XPAnchor 
			//worldOriginHelper.SetWorldOrigin(cloudAnchor.transform);

			var OGAnchor = playArea.GetComponentInParent<Anchor>();

			// Detach City-Containing Play Area from the Anchor.
			playArea.transform.SetParent(cloudAnchor.transform, true);

			// Destroy Original Anchor (Not sure if ideal)
			Destroy(OGAnchor.gameObject);

			// Notify the connected clients through CityTfBehaviour function.
			localUser.CityTfBehaviour.OnPlayAreaConfirm();

		}
		else {
			if (result.Response == CloudServiceResponse.ErrorDatasetInadequate) {
				Debug.Log($"[{this.GetType().Name}] Can't Host Anchor: Error Dataset Inadequate!");
				ARSceneCtrl._ShowAndroidToastMessage("Scan more of the surroundings!");
				tryHostBtn.ShowButton(true);
			}
			else {
				Debug.LogError($"[PlayAreaManager] Hosting Anchor Failed! Cloud Service Response: " +
				$"{result.Response.ToString()}");
			}
		}
	}

	/// <summary>
	/// Called by the ARSceneController.
	/// Starts to attempt resolving of the hosted Cloud Anchor from the given CloudId.
	/// </summary>
	/// <param name="CloudId"> The Hosted Cloud Anchor's Id. Accessed by CloudAnchorResult.Anchor.CloudId.</param>
	public void StartResolving(string CloudId) {
		Debug.Log($"[{this.GetType().Name}] Starting Anchor Resolution Attempt...");
		ARSceneCtrl._ShowAndroidToastMessage("Attempting to resolve Anchor...");
		XPSession.ResolveCloudAnchor(CloudId).ThenAction(OnResolveComplete);
	}

	/// <summary>
	/// Callback after ResolveCloudAnchor() is completed. Returns either success or an error.
	/// </summary>
	/// <param name="result"> The Resolved Cloud Anchor and Service Response. Anchor is Null if Response is an error.</param>
	private void OnResolveComplete(CloudAnchorResult result) {
		// Instantiate Play Area Object on Resolved Anchor Object
		if (result.Response == CloudServiceResponse.Success) {
			Debug.Log($"[{this.GetType().Name}] Resolve Successs");
			ARSceneCtrl._ShowAndroidToastMessage("Anchor Resolution Success");
			// For debugging only.
			//Instantiate(XPAnchorVisualizer, result.Anchor.transform);

			// Set world origin to XPAnchor
			this.cloudAnchor = result.Anchor;
			//worldOriginHelper.SetWorldOrigin(cloudAnchor.transform);

			// Request PlayAreaTf from Server. If request success, leads to ClientCreatePlayArea().
			localUser.CityTfBehaviour.RequestPlayAreaTf();
		}
		else {
			Debug.LogError($"[PlayAreaManager] Resolving Anchor Failed! Cloud Service Response: " +
				$"{result.Response.ToString()}");
		}
	}
}

