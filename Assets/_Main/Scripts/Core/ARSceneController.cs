using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using GoogleARCore;
using GoogleARCore.Examples.Common;
using FixCityAR;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public enum ApplicationMode {
	Host,
	Client
}

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

	private bool m_allowManipulation;
	public bool AllowManipulation {
		get {
			return m_allowManipulation;
		}
		private set {
			m_allowManipulation = value;
		}
	}

	private ControlsManager controlsManager;
	private PlayAreaManager PAManager;
	private CityManager cityManager;

	private bool planeDiscoveryRefreshed = false;
	private bool m_IsQuitting = false;
	private bool DepthMenuOpened = false;

	private IssueObject selectedIssue;

	// Network-related fields and attributes
	public ARUser localUser;
	public ApplicationMode applicationMode;
	public string AnchorId {
		get;
		private set;
	}


	void Awake()
    {
		// Enable ARCore to target 60fps camera capture frame rate on supported devices.
		// Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
		Application.targetFrameRate = 60;
		sharedInstance = this;
	}

	private void Start() {
		controlsManager = ControlsManager.Instance;
		PAManager = PlayAreaManager.Instance;
		cityManager = CityManager.Instance;
		// Hide City Manager on startup.
		cityManager.gameObject.SetActive(false);

		if (PerformanceTesting.IsEvaluating) {
			DebugOverlay.Instance.ARSceneTime = Time.time;
			DebugOverlay.Instance.ARSceneFrame = Time.frameCount;
		}

		if (applicationMode == ApplicationMode.Host) {
			// Start Hosting Process
			PAManager.Select();
			InstructionPanel.Instance.ShowInstruction(InstructionConstants.SET_TR);
			//StartCoroutine(ExecuteAfterTime(3f, ()=>InstructionPanel.Instance.ShowInstruction(InstructionConstants.SET_TR)));
		}
		// Client begins in ARUser.Start() -> "RequestCloudId"
	}

	// Update is called once per frame
	void Update() {
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
			DepthMenuOpened = true;
			DepthSettings.ConfigureDepthBeforePlacingFirstAsset();
			return;
		}

		if (CityManager.Instance.b_IsCityPlaced && Input.GetMouseButtonDown(0)) {
			// Handle issue canvas opening/closing
			AllowManipulation = false;
			LayerMask layerMask = LayerMask.GetMask("Issue","Pinnables");
			Ray r = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
			RaycastHit p_hit;
			if (Physics.Raycast(r, out p_hit, Mathf.Infinity, layerMask.value)) {
				var iobj = p_hit.transform.GetComponentInParent<IssueObject>();
				if (iobj != null) {
					if (selectedIssue == null) {
						// Select new Issue
						selectedIssue = iobj;
						selectedIssue.ToggleCanvas();
					}
					else if (selectedIssue == iobj) {
						// Disable selected issue
						selectedIssue.ToggleCanvas();
						selectedIssue = null;
					}
					return;
				}
				
				if (p_hit.collider.CompareTag("InfoPin")) {
					var infoPin = p_hit.collider.GetComponent<InfoPin>();
					infoPin.ToggleShowCanvas();

					return;
				}

				if (p_hit.collider.CompareTag("Ping")) {
					var ping = p_hit.collider.GetComponent<PingObject>();
					localUser.ToolBehaviour.pingReference.GetComponent<PingObject>().UnPinToCity();
					localUser.ToolBehaviour.pingReference = null;
					return;
				}
			}
		}

		AllowManipulation = true;
	}

	#region Play Area Functions / Callbacks

	public void OnPlayAreaConfirmed(Bounds playAreaBounds, PlayAreaManager PAmngr) {
		cityManager.gameObject.SetActive(true);

		#region City Model Transform re-adjustment
		// Rescale City to Fit inside Play Area Bounds
		Bounds cityBounds = cityManager.Bounds;
		var x_ratio = playAreaBounds.size.x / cityBounds.size.x;
		var z_ratio = playAreaBounds.size.z / cityBounds.size.z;
		var min_ratio = Mathf.Min(x_ratio, z_ratio);
		var y_scale_multiplier = PAmngr.PlayArea.MeshBoundary.transform.localScale.y;
		Vector3 newScale = new Vector3(cityManager.transform.localScale.x * min_ratio,
			cityManager.transform.localScale.y * min_ratio,
			cityManager.transform.localScale.z * min_ratio);
		cityManager.transform.localScale = newScale;

		// Attach City as a child of the Play Area
		cityManager.transform.SetParent(PAmngr.PlayArea.CityContainer.transform);

		// Reset Local Position and Local Rotation to Vector3.Zero; Local Scale to 1, and set CityContainer to City's previous scale.
		cityManager.transform.localPosition = Vector3.zero;
		cityManager.transform.localEulerAngles = Vector3.zero;
		PAmngr.PlayArea.CityContainer.transform.localScale = cityManager.transform.localScale;
		cityManager.transform.localScale = Vector3.one;
		#endregion

		// Callback functions for the other components
		cityManager.OnCityAttached();
		EnablePlaneDiscoveryGuide(false);
		SettingsMenu.Instance.EnableResetCityPlacement(true);
		controlsManager.ShowControls(true);
		
		if (PerformanceTesting.IsEvaluating) {
			DebugOverlay.Instance.SaveFrameCount(FrameCounts.cityPlaced);
			DebugOverlay.Instance.CityPlacedTime = Time.time;
			DebugOverlay.Instance.SetAverageFPS(AvgFPS.ARScene);
		}
	}

	/// <summary>
	/// Called on the client devices.There are no PABounds as the City Transform might have been manipulated
	/// before the client connected. Instead, determined Transform values are passed with the cityTf parameter.
	/// </summary>
	/// <param name="PAmngr"></param>
	public void OnPlayAreaConfirmed(TfValues cityTf, PlayAreaManager PAmngr) {
		cityManager.gameObject.SetActive(true);

		#region City Model Transform re-adjustment
		// Attach City as a child of the Play Area
		cityManager.transform.SetParent(PAmngr.PlayArea.CityContainer.transform);

		// Adjust City Transform to reflect the server's City Transform.
		cityManager.transform.localPosition = cityTf.localPosition;
		cityManager.transform.localRotation = cityTf.localRotation;
		cityManager.transform.localScale = cityTf.localScale;
		#endregion

		// Callback functions for the other components
		cityManager.OnCityAttached();
		EnablePlaneDiscoveryGuide(false);
		SettingsMenu.Instance.EnableResetCityPlacement(true);
		controlsManager.ShowControls(true);

		if (PerformanceTesting.IsEvaluating) {
			DebugOverlay.Instance.SaveFrameCount(FrameCounts.cityPlaced);
			DebugOverlay.Instance.CityPlacedTime = Time.time;
			DebugOverlay.Instance.SetAverageFPS(AvgFPS.ARScene);
		}
	}


	public void ResetPlayAreaPlacement() {
		EnablePlaneDiscoveryGuide(true);
		controlsManager.ShowControls(false);
		if (selectedIssue != null) {
			selectedIssue.DisableCanvas();
			selectedIssue = null;
		}
		PlayAreaManager.Instance.RemovePlacement();


		if (PerformanceTesting.IsEvaluating) {
			DebugOverlay.Instance.SaveFrameCount(FrameCounts.cityRemoved);
			DebugOverlay.Instance.CityRemovedTime = Time.time;
			DebugOverlay.Instance.SetAverageFPS(AvgFPS.CityPlaced);
		}

		_ShowAndroidToastMessage("Tap on a Detected Plane to place the Interactable Area.");
	}
	#endregion

	/// <summary>
	/// For Host: Called after Cloud Anchor has been successfully published.
	/// For Client: Called right after the client joins the server.
	/// </summary>
	/// <param name="anchorId"></param>
	public void SetCloudAnchorId(string anchorId) {
		AnchorId = anchorId;
		// Empty means PlayArea Placement was removed. Thus, no further action is required.
		if (anchorId != "") {
			AnchorId = anchorId;
			if (applicationMode == ApplicationMode.Host) {
				// Advertise Server
				var netDiscovery = FindObjectOfType<NewNetworkDiscovery>();
				netDiscovery.AdvertiseServer();
			}
			else {
				// Allow Play Area Manager to Resolve
				PAManager.StartResolving(AnchorId);
			}
		}
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
	public void _ShowAndroidToastMessage(string message) {
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
		function();
	}

	#endregion
}
