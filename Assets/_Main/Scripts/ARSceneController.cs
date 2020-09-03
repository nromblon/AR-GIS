using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using GoogleARCore;
using GoogleARCore.Examples.Common;
using GoogleARCore.Examples.ObjectManipulation;
using FixCityAR;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class ARSceneController : MonoBehaviour {
	private static ARSceneController sharedInstance;
	public static ARSceneController Instance {
		get {
			return sharedInstance;
		}
	}

	public DepthSettings DepthSettings;
	public Camera ARCamera;
	public PlaneDiscoveryGuide planeDiscoveryGuide;
	public DetectedPlaneGenerator planeGenerator;

	private bool isPlaneDiscoveryGuideActive;
	public bool IsPlaneDiscoveryGuideActive {
		get {
			return isPlaneDiscoveryGuideActive;
		}
	}

	private ControlsManager controlsManager;

	private bool planeDiscoveryRefreshed = false;
	private bool m_IsQuitting = false;
	private bool DepthMenuOpened = false;

	
    void Awake()
    {
		// Enable ARCore to target 60fps camera capture frame rate on supported devices.
		// Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
		Application.targetFrameRate = 60;
		sharedInstance = this;
	}

	private void Start() {
		controlsManager = ControlsManager.Instance;
	}

	// Update is called once per frame
	void Update()
    {
		_UpdateApplicationLifecycle();

		//// Checks if Depth Menu windows are open
		//if (DepthMenu != null && !DepthMenu.CanPlaceAsset()) {
		//	return false;
		//}

		//// If the player has not touched the screen, we are done with this update.
		//if (Input.touchCount < 1) {
		//	return false;
		//}

		if (!DepthMenuOpened && DepthSettings != null) {
			// This enables Plane Discovery Guide after depth menu has been configured.
			Debug.Log("Depth Menu needs configuration first.");
			DepthMenuOpened = true;
			DepthSettings.ConfigureDepthBeforePlacingFirstAsset();
			return;
		}

	}

	public void OnPlayAreaConfirmed(Bounds playAreaBounds, PlayAreaManager PAmngr) {
		CityGMLManager cityGMLMngr = CityGMLManager.Instance;
		cityGMLMngr.gameObject.SetActive(true);

		// Resize city bounds
		Bounds cityBounds = cityGMLMngr.Bounds;
		var x_ratio = playAreaBounds.size.x / cityBounds.size.x;
		var z_ratio = playAreaBounds.size.z / cityBounds.size.z;
		var min_ratio = Mathf.Min(x_ratio, z_ratio);
		var y_scale_multiplier = PAmngr.PlayArea.MeshBoundary.transform.localScale.y;
		Vector3 newScale = new Vector3(cityGMLMngr.transform.localScale.x * min_ratio,
			cityGMLMngr.transform.localScale.y * min_ratio,
			cityGMLMngr.transform.localScale.z * min_ratio);

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
		
		cityGMLMngr.OnCityPlaced();
		EnablePlaneDiscoveryGuide(false);
		SettingsMenu.Instance.EnableResetCityPlacement(true);
		controlsManager.ShowControls(true);
	}

	public void ResetPlayAreaPlacement() {
		EnablePlaneDiscoveryGuide(true);
		controlsManager.ShowControls(false);
		PlayAreaManager.Instance.RemovePlacement();
		Debug.Log("Reset Play Area Placement");

		_ShowAndroidToastMessage("Tap on a Detected Plane to place the Interactable Area.");
	}

	public void EnablePlaneDiscoveryGuide(bool val) {
		planeDiscoveryGuide.EnablePlaneDiscoveryGuide(val);
		planeGenerator.gameObject.SetActive(val);
		isPlaneDiscoveryGuideActive = val;
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

	IEnumerator ExecuteAfterTime(float time, Action function) {
		yield return new WaitForSecondsRealtime(time);
		Debug.Log("Call function");
		function();
	}
	#endregion
}
