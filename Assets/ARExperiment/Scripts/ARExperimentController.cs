using System.Collections.Generic;
using GoogleARCore;
using GoogleARCore.Examples.Common;
using UnityEngine;
using UnityEngine.EventSystems;

using Assets.Scripts.CityGML2GO;

using UnityEngine.SceneManagement;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class ARExperimentController : MonoBehaviour
{
	/// <summary>
	/// The first-person camera being used to render the passthrough camera image (i.e. AR
	/// background).
	/// </summary>
	public Camera FirstPersonCamera;

	public GameObject CGMLGO;

	public GameObject PingPrefab;

	public GameObject cube;

	public GameObject AlertPrefab;

	public DepthMenu DepthMenu;

	private TownController townController;

	private GameObject currentPin;
	private IssueObject openedAlert;

	/// <summary>
	/// True if the app is in the process of quitting due to an ARCore connection error,
	/// otherwise false.
	/// </summary>
	private bool m_IsQuitting = false;

	private bool hasPlacedTown = false;
	private bool scaleMode = false;

	public GameObject PlayAreaSelectorPrefab;

	public void Awake() {
		// Enable ARCore to target 60fps camera capture frame rate on supported devices.
		// Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
		Application.targetFrameRate = 60;
	}

	private void Start() {
		//SceneManager.MoveGameObjectToScene
	}

	// Update is called once per frame
	void Update() {
		_UpdateApplicationLifecycle();

		if (DepthMenu != null && !DepthMenu.CanPlaceAsset()) {
			return;
		}

		// If the player has not touched the screen, we are done with this update.
		if (Input.touchCount < 1) {
			return;
		}

		Touch touch = Input.GetTouch(0);

		// Should not handle input if the player is pointing on UI.
		if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) {
			return;
		}

		//// Pinch to scale actions in this block.
		//if (scaleMode && Input.touchCount > 1) {
		//	Touch touch1 = Input.GetTouch(1);

		//	townController.PinchToScale(touch, touch1);

		//	return;
		//}

		// If using two fingers, start scale mode and enable town outline
		if (!scaleMode && hasPlacedTown && Input.touchCount > 1) {
			scaleMode = true;
			return;
		}

		// If using less than two fingers, cancel scale mode and disable town outline
		if (scaleMode) {
			if (Input.touchCount < 2) {
				scaleMode = false;
			}
			return;
		}

		// If Touch input is not a 'tap', do not proceed to placing pins/town
		if (touch.phase != TouchPhase.Began) {
			return;
		}

		if (hasPlacedTown) {
			Ray r = Camera.main.ScreenPointToRay(touch.position);
			RaycastHit p_hit;
			if(Physics.Raycast(r, out p_hit)){
				Debug.Log("Raycast sending for alertpin");
				if (p_hit.transform.gameObject.tag == "Alert") {
					IssueObject alertPin = p_hit.transform.GetComponentInParent<IssueObject>();
					Debug.Log("Alert Pin Hit: "+alertPin);
					if (alertPin.ToggleCanvas()) {
						openedAlert = alertPin;
						Debug.Log("Alert Pin Opened");
					}
					else {
						Debug.Log("Alert Pin Closed");
						openedAlert = null;
					}

					return;
				}
				else {
					if(openedAlert != null) {
						openedAlert.DisableCanvas();
						openedAlert = null;
					}
				}
			}
		}

		// Raycast against the location the player touched to search for planes.
		TrackableHit hit;
		TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
			TrackableHitFlags.FeaturePointWithSurfaceNormal;

		if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit)) {
			// Use hit pose and camera pose to check if hittest is from the
			// back of the plane, if it is, no need to create the anchor.
			if ((hit.Trackable is DetectedPlane) &&
				Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
					hit.Pose.rotation * Vector3.up) < 0) {
				Debug.Log("Hit at back of the current DetectedPlane");
			}
			else {
				if (DepthMenu != null) {
					// Show depth card window if necessary.
					DepthMenu.ConfigureDepthBeforePlacingFirstAsset();
				}

				
				GameObject prefab;
				if (hit.Trackable is DetectedPlane) {
					DetectedPlane detectedPlane = hit.Trackable as DetectedPlane;

					if (detectedPlane.PlaneType == DetectedPlaneType.HorizontalUpwardFacing) {

						if (!hasPlacedTown) {

							// Anchor the town object
							if(CGMLGO == null) {
								CGMLGO = CityManager.Instance.gameObject;
							}


							CGMLGO.SetActive(true);
							AnchorCity(hit);
							//CGMLGO.GetComponent<CityGml2GO>().InstantiateCity();
							//prefab = TowmModel;
							CGMLGO.GetComponent<CityGml2GO>().RefreshMeshes();
							//AnchorObject(prefab, hit);
							//townController = town.GetComponent<TownController>();


							prefab = cube;
							AnchorObject(prefab, hit);

							hasPlacedTown = true;
						}
						else {
							if(currentPin != null) {
								// Remove anchor of old pin
								Destroy(currentPin.transform.parent.gameObject);
							}

							// Set a pin
							// Do a raycast on 
							prefab = PingPrefab;

							// do raycast on mesh of town
							// if hit Tag == "Town", change y value of anchor to that.
							currentPin = AnchorObject(prefab, hit);
						}
					}
				}
			}
		}

	}

	private void AnchorCity(TrackableHit hit) {
		// Create an anchor to allow ARCore to track the hitpoint as understanding of
		// the physical world evolves.
		var anchor = hit.Trackable.CreateAnchor(hit.Pose);

		// Compensate for the hitPose rotation facing away from the raycast (i.e.
		// camera).
		CGMLGO.transform.localScale = new Vector3(.01f, .01f, .01f);
		CGMLGO.transform.position = hit.Pose.position;
		CGMLGO.transform.rotation = hit.Pose.rotation;

		CGMLGO.transform.Rotate(0, 0, 0, Space.Self);

		CGMLGO.transform.parent = anchor.transform;
	}

	/// <summary>
	/// Creates an anchored object based on a hit trackable point .
	///
	/// </summary>
	/// <param name="toAnchor"></param>
	/// <param name="hit"></param>
	private GameObject AnchorObject(GameObject toAnchor, TrackableHit hit) {
		// Instantiate prefab at the hit pose.
		var gameObject = Instantiate(toAnchor, hit.Pose.position, hit.Pose.rotation);

		// Compensate for the hitPose rotation facing away from the raycast (i.e.
		// camera).
		gameObject.transform.Rotate(0, 0, 0, Space.Self);

		// Create an anchor to allow ARCore to track the hitpoint as understanding of
		// the physical world evolves.
		var anchor = hit.Trackable.CreateAnchor(hit.Pose);

		// Make game object a child of the anchor.
		gameObject.transform.parent = anchor.transform;


		return gameObject;
	}

	/// <summary>
	/// Check and update the application lifecycle.
	/// Checks Motion Tracking
	/// </summary>
	private void _UpdateApplicationLifecycle() {
		// Exit the app when the 'back' button is pressed.
		if (Input.GetKey(KeyCode.Escape)) {
			Application.Quit();
		}

		// Only allow the screen to sleep when not tracking.
		if (Session.Status != SessionStatus.Tracking) {
			Screen.sleepTimeout = SleepTimeout.SystemSetting;
		}
		else {
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
		}

		if (m_IsQuitting) {
			return;
		}

		// Quit if ARCore was unable to connect and give Unity some time for the toast to
		// appear.
		if (Session.Status == SessionStatus.ErrorPermissionNotGranted) {
			_ShowAndroidToastMessage("Camera permission is needed to run this application.");
			m_IsQuitting = true;
			Invoke("_DoQuit", 0.5f);
		}
		else if (Session.Status.IsError()) {
			_ShowAndroidToastMessage(
				"ARCore encountered a problem connecting.  Please start the app again.");
			m_IsQuitting = true;
			Invoke("_DoQuit", 0.5f);
		}
	}

	/// <summary>
	/// Actually quit the application.
	/// </summary>
	private void _DoQuit() {
		Application.Quit();
	}

	/// <summary>
	/// Show an Android toast message.
	/// </summary>
	/// <param name="message">Message string to show in the toast.</param>
	private void _ShowAndroidToastMessage(string message) {
		AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject unityActivity =
			unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

		if (unityActivity != null) {
			AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
			unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
			{
				AndroidJavaObject toastObject =
					toastClass.CallStatic<AndroidJavaObject>(
						"makeText", unityActivity, message, 0);
				toastObject.Call("show");
			}));
		}
	}
}
