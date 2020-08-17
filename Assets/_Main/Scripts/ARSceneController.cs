using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using GoogleARCore;
using GoogleARCore.Examples.Common;
using FixCityAR;

public class ARSceneController : MonoBehaviour
{
	private static ARSceneController sharedInstance;
	public static ARSceneController Instance {
		get {
			return sharedInstance;
		}
	}

	public DepthMenu depthMenu;
	public Camera ARCamera;

	private bool m_IsQuitting = false;
	
    void Awake()
    {
		// Enable ARCore to target 60fps camera capture frame rate on supported devices.
		// Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
		Application.targetFrameRate = 60;
		sharedInstance = this;
	}

    // Update is called once per frame
    void Update()
    {
		_UpdateApplicationLifecycle();

		#region Input pre-checks
		if (depthMenu != null && !depthMenu.CanPlaceAsset()) {
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
		#endregion


	}

	/// <summary>
	/// Creates an anchored object based on a hit trackable point.
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

	public void OnPlayAreaConfirmed(Bounds playAreaBounds, PlayAreaManager PAmngr) {
		CityGMLManager cityGMLMngr = CityGMLManager.Instance;
		cityGMLMngr.gameObject.SetActive(true);

		// Resize city bounds
		Bounds cityBounds = cityGMLMngr.Bounds;
		var x_ratio = playAreaBounds.size.x / cityBounds.size.x;
		var z_ratio = playAreaBounds.size.z / cityBounds.size.z;
		var min_ratio = Mathf.Min(x_ratio, z_ratio);
		Vector3 newScale = cityGMLMngr.transform.localScale * min_ratio;

		//Debug.Log("City Bounds center: " + cityBounds.center);
		cityGMLMngr.transform.localScale = newScale;
		
		// set as child of play area
		PAmngr.PlayArea.SetAsChild(cityGMLMngr.gameObject);

		// set local position and localrotate = 0
		cityGMLMngr.transform.localPosition = Vector3.zero;
		cityGMLMngr.transform.localEulerAngles = Vector3.zero;
		//var city_center = cityGMLMngr.Bounds.center;
		//Debug.Log("City Bounds center_rescaled: " + city_center);
		//city_center = Vector3.Scale(city_center, cityGMLMngr.transform.localScale);
		//Debug.Log("City Bounds center outside of cityGO: " + city_center.ToString("F4"));
		//cityGMLMngr.transform.localPosition = city_center;

		//cityGMLMngr.transform.Translate(Vector3.Scale(cityBounds.center,newScale));
		//cityGMLMngr.transform.localPosition = pos;
		// Deselect PlayArea
		PAmngr.PlayArea.Deselect();
	}

	#region Application Essentials
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
	/// Show an Android toast message.
	/// </summary>
	/// <param name="message">Message string to show in the toast.</param>
	private void _ShowAndroidToastMessage(string message) {
		AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject unityActivity =
			unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

		if (unityActivity != null) {
			AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
			unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
				AndroidJavaObject toastObject =
					toastClass.CallStatic<AndroidJavaObject>(
						"makeText", unityActivity, message, 0);
				toastObject.Call("show");
			}));
		}
	}
	#endregion
}
