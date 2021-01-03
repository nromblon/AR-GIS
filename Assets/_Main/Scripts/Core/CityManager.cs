using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.CityGML2GO;
using FixCityAR;
using GoogleARCore.Examples.ObjectManipulation;
using Assets.Scripts.CityGML2GO.Scripts;
using System.Collections;

public enum GenerateColliderOptions {
	DoNotGenerate = 0,
	GenerateBoxParent = 1,
	GenerateBoxChildren = 2,
	GenerateMesh = 3
}

public class CityManager : MonoBehaviour
{
	const string GCS_CONVERT_CODE = "4326";

	private static CityManager sharedInstance;
	public static CityManager Instance {
		get {
			return sharedInstance;
		}
	}

	[Header("Config")]
	public float y_offset = -5;
	public CityGml2GO cityGML2GO;
	public IssueManager IssueManager;
	public GenerateColliderOptions GenerateColliders = 0;
	public bool shouldRemoveOutliers = true;
	public float outlierCoeff = 3f;
	public float syncUpdateDuration = 1f;

	private Bounds bounds;
	public Bounds Bounds {
		get {
			//var origScale = transform.localScale;
			var origRotation = transform.localRotation;

			transform.localEulerAngles = Vector3.zero;
			//transform.localScale = Vector3.one;

			bounds = Utilities.GetBounds(gameObject);

			transform.localRotation = origRotation;
			//transform.localScale = origScale;

			return bounds;
		}
	}

	[Header("Debugging")]
	public Manipulator[] Manipulators;
	public bool b_IsCityPlaced;
	private BuildingProperties[] buildings;
	[SerializeField] private Vector3 originOnPlaced;
	[SerializeField] private Vector3 onLoadPosition;
	[SerializeField] private Vector3 onLoadScale;

	private double coord_distance;
	public double UnitPerLatLongRatio {
		get {
			double v_distance = Utilities.ComputeDistance(transform.position, Bounds.max);
			return v_distance / coord_distance;
		}
	}

	// For Networking Purposes. Should only be set to true if manipulation is done.
	public bool HasManipulationPerformed = false;
	// True if the local user is currently manipulating the city object.
	public bool IsManipulating = false;
	// Reference for the currently running LerpToTransform Coroutine
	private Coroutine runningSyncCouroutine;

	private void Awake() {
		sharedInstance = this;
		Manipulators = GetComponents<Manipulator>();

		// Make sure that the manipulators are disabled and deselected.
		foreach (var m in Manipulators) {
			m.enabled = false;
		}
	}

	// Start is called before the first frame update
	void Start()
    {
		this.cityGML2GO = GetComponentInChildren<CityGml2GO>();
		if (IssueManager == null)
			IssueManager = IssueManager.Instance;

		b_IsCityPlaced = false;

    }

	#region Initialization Methods
	public void InitializeCity() {
		if (PerformanceTesting.IsEvaluating) {
			DebugOverlay.Instance.SetStopwatch(FrameCounts.cityInitStart);
			DebugOverlay.Instance.SaveFrameCount(FrameCounts.cityInitStart);
		}
		((NewNetworkManager)NewNetworkManager.singleton).loadedFiles = cityGML2GO.LoadedFiles;

		// Recenter first level children
		RecenterChildren();
		
		// recenter buildings
		RecenterBuildings();

		// Generate Colliders
		GenerateBuildingColliders();

		// Initialize Alerts
		InitializeIssues();

		onLoadPosition = transform.position;
		onLoadScale = transform.localScale;

		if (PerformanceTesting.IsEvaluating) {
			DebugOverlay.Instance.SaveFrameCount(FrameCounts.cityInitEnd);
			DebugOverlay.Instance.SetStopwatch(FrameCounts.cityInitEnd);
			DebugOverlay.Instance.SetAverageFPS(AvgFPS.CityInit);
			int triCount = Utilities.GetTriCount(gameObject);
			DebugOverlay.Instance.SetTriCount(triCount);

			DebugOverlay.Instance.CityInitEndTime = Time.time;
		}
	}

	/// <summary>
	/// Recenters First level Children and adds a BuildingController component on each.
	/// </summary>
	private void RecenterChildren() {
		var childCount = transform.childCount;
		var children = new List<Transform>();
		// Initialize List
		foreach (var i in Enumerable.Range(0, childCount)) {
			children.Add(transform.GetChild(i));
		}
		
		var center = Bounds.center;

		// Detach 1st level children
		foreach (var child in children) {
			child.parent = null;
		}

		transform.position = new Vector3(center.x, transform.position.y, center.z);

		// Reattach children
		foreach(var child in children) {
			child.SetParent(transform);
		}
		// reset position
		transform.position = Vector3.zero;

		// Add BuildingController Component
		foreach (var child in children) {
			child.gameObject.AddComponent<BuildingController>();
		}
	}

	private void RecenterBuildings() {
		if (buildings == null) {
			buildings = GetComponentsInChildren<BuildingProperties>();
		}
		var children = new List<Transform>();
		foreach (var building in buildings) {
			if (shouldRemoveOutliers) {
				RemoveOutlierMeshes(building);
			}
			var go = building.gameObject;

			// Set building's layer to be the same as root.
			go.layer = this.gameObject.layer;

			Bounds b = Utilities.GetBounds(go);
			var childCount = go.transform.childCount;
			// Initialize Children List
			for (int i = 0; i < childCount; i++) {
				var child = go.transform.GetChild(i);
				children.Add(child);

				// Set child layer to be the same as root.
				child.gameObject.layer = this.gameObject.layer;
			}
			// Detach Children
			foreach (var child in children) {
				child.parent = null;
			}
			// recenter parent
			go.transform.position = b.center;

		
			// Reattach children
			foreach (var child in children) {
				child.SetParent(go.transform);
			}
			children.Clear();

			// add y-offset
			go.transform.localPosition = new Vector3(go.transform.localPosition.x,
				go.transform.localPosition.y + y_offset,
				go.transform.localPosition.z);
		}
		children = null;
	}

	private void GenerateBuildingColliders() {
		switch (GenerateColliders) {
			case (GenerateColliderOptions.GenerateBoxParent):
				foreach (var building in buildings) {
					var buildingGo = building.gameObject;
					var box = buildingGo.AddComponent<BoxCollider>();
					box.isTrigger = true;
					Utilities.FitColliderToChildren(buildingGo);
				}
				break;

			case (GenerateColliderOptions.GenerateBoxChildren):
				foreach (var building in buildings) {
					int childCount = building.transform.childCount;
					for (int i = 0; i < childCount; i++) {
						var child = building.transform.GetChild(i);
						var box = child.gameObject.AddComponent<BoxCollider>();
						box.isTrigger = true;
						Utilities.FitColliderToChildren(child.gameObject);
					}
				}
				break;

			case (GenerateColliderOptions.GenerateMesh):
				// TODO
				break;

			default:
				break;
		}
	}

	private void RemoveOutlierMeshes(BuildingProperties building) {
		List<Vector3> childCenters = Utilities.GetBoundCenters(building.gameObject);

		Vector3 centroid = Vector3.zero;
		foreach (var c in childCenters) {
			centroid += c;
		}
		centroid = new Vector3(centroid.x / childCenters.Count,
			centroid.y / childCenters.Count,
			centroid.z / childCenters.Count);

		float stDev;
		float sum = 0;
		foreach (var c in childCenters) {
			sum += Mathf.Pow(centroid.x - c.x, 2)
				+ Mathf.Pow(centroid.y - c.y, 2)
				+ Mathf.Pow(centroid.z - c.z, 2);
		}
		stDev = sum / childCenters.Count;
		stDev = Mathf.Sqrt(stDev);

		float maxDistance = outlierCoeff * stDev;
		for(int i = 0; i < childCenters.Count; i ++) {
			var c = childCenters[i];
			var distance = Mathf.Abs(Vector3.Distance(c, centroid));
			if (distance > maxDistance) {
				Destroy(building.transform.GetChild(i).gameObject);
			}
		}
	}
	
	private void InitializeIssues() {
		// Convert Coordinates to Lat/Long system
		Coordinates[] cityCoords = new Coordinates[3];
		cityCoords[0] = CityProperties.raw_MinPoint;
		cityCoords[2] = CityProperties.raw_MaxPoint;
		cityCoords[1] = CityProperties.raw_Center;

		CoordinateConverter cc = new CoordinateConverter();
		StartCoroutine(cc.ConvertCoordinate(cityCoords, GCS_CONVERT_CODE));

		StartCoroutine(WaitForCoordinateConverterResults(cc, () => {
			Coordinates[] wgs_coords = cc.results;
			CityProperties.wgs_MinPoint = wgs_coords[0];
			CityProperties.wgs_MaxPoint = wgs_coords[2];

			// Compute coord_distance to be used in Conversion Ratio for LatLong to Unity Units
			GPSEncoder.SetLocalOrigin(new Vector2((float)CityProperties.wgs_Center.y, (float)CityProperties.wgs_Center.x));

			IssueManager.InitializeIssues();

			// Set Issuemanager as child
			IssueManager.gameObject.transform.SetParent(transform);
		}));
	}
	#endregion

	public void OnCityAttached() {
		b_IsCityPlaced = true;

		var scaleManipulator = GetComponent<ExtendedScaleManipulator>();
		var curScale = transform.localScale.x;

		scaleManipulator.SetMinMax(curScale, curScale * 8);
		Select();
		originOnPlaced = transform.localPosition;
	}

	public void DetachCity() {
		Deselect();

		b_IsCityPlaced = false;
		transform.parent = null;
		transform.localScale = onLoadScale;
		transform.position = onLoadPosition;
		transform.localEulerAngles = Vector3.zero;

		foreach(var bc in GetComponentsInChildren<BuildingController>()) {
			bc.EnableRenderers(true);
		}

		gameObject.SetActive(false);
	}

	public void Select() {
		foreach (var m in Manipulators) {
			m.enabled = true;
			m.Select();
		}
	}

	public void Deselect() {
		foreach (var m in Manipulators) {
			m.Deselect();
			m.enabled = false;
		}
	}

	/// <summary>
	/// Called by the client if someone on the network has updated their City Module's transform.
	/// Called every update if transform.hasChanged = true in CityTransformBehaviour.
	/// </summary>
	/// <param name="cityTf"> The values for the changed city transform. </param>
	public void ClientUpdateTransform(TfValues cityTf) {
		StartCoroutine(LerpToTransform(cityTf,syncUpdateDuration));

	}

	public void ResetPositionToOrigin(float duration) {
		// If this client is currently manipulating, ignore incoming Rpc.
		if (IsManipulating)
			return;

		// If Tf is already syncing, stop and resync to new instruction.
		if (runningSyncCouroutine != null)
			StopCoroutine(runningSyncCouroutine);

		runningSyncCouroutine = StartCoroutine(LerpPositionToOrigin(duration));
	}

	IEnumerator WaitForCoordinateConverterResults(CoordinateConverter cc, System.Action callback) {
		yield return new WaitUntil(() => cc.isDone);
		callback();
	}

	IEnumerator LerpPositionToOrigin(float duration) {
		// Disable touch controls
		ManipulationSystem.Instance.Deselect();
		float time = 0;
		float t;
		Vector3 startPosition = transform.localPosition;
		while(time < duration) {
			t = time / duration;
			// Smooth-step Lerp
			t = t * t * (3f - 2f * t);
			transform.localPosition = Vector3.Lerp(startPosition, originOnPlaced, t);
			time += Time.deltaTime;
			yield return null;
		}
		transform.localPosition = originOnPlaced;
		// Reenable touch controls
		Select();
		ControlsManager.Instance.EnableRecenterButton(true);
	}

	IEnumerator LerpToTransform(TfValues toTf, float duration) {
		// Disable touch controls
		ManipulationSystem.Instance.Deselect();
		float time = 0;
		float t;
		Vector3 startPosition = transform.localPosition;
		Quaternion startRotation = transform.localRotation;
		Vector3 startScale = transform.localScale;
		while (time < duration) {
			t = time / duration;
			// Smooth-step Lerp
			t = t * t * (3f - 2f * t);
			transform.localPosition = Vector3.Lerp(startPosition, toTf.localPosition, t);
			transform.localRotation = Quaternion.Lerp(startRotation, toTf.localRotation, t);
			transform.localScale = Vector3.Lerp(startScale, toTf.localScale, t);
			time += Time.deltaTime;
			yield return null;
		}
		transform.localPosition = originOnPlaced;
		// Reenable touch controls
		Select();
		ControlsManager.Instance.EnableRecenterButton(true);

		runningSyncCouroutine = null;
	}
}

public static class CityProperties {
	/// <summary>
	/// Minpoint and Maxpoint in Raw format (as read in the GML file).
	/// </summary>
	public static Coordinates raw_MinPoint, raw_MaxPoint;
	private static bool HasInitMin = false, HasInitMax = false;
	public static Coordinates raw_Center {
		get {
			double center_x = (raw_MaxPoint.x + raw_MinPoint.x) / 2;
			double center_y = (raw_MaxPoint.y + raw_MinPoint.y) / 2;
			double center_elev = (raw_MaxPoint.z + raw_MinPoint.z) / 2;
			return new Coordinates(center_x, center_y, center_elev, raw_MaxPoint.gcs_type);
		}
	}

	/// <summary>
	/// Minpoint and Maxpoint in WGS84 format (lat/long degrees).
	/// </summary>
	public static Coordinates wgs_MinPoint, wgs_MaxPoint;
	public static Coordinates wgs_Center {
		get {
			double center_x = (wgs_MaxPoint.x + wgs_MinPoint.x) / 2;
			double center_y = (wgs_MaxPoint.y + wgs_MinPoint.y) / 2;
			double center_elev = (wgs_MaxPoint.z + wgs_MinPoint.z) / 2;
			return new Coordinates(center_x, center_y, center_elev, wgs_MaxPoint.gcs_type);
		}
	}

	public static void CheckSetRawMinPoint(Coordinates point) {
		if (!HasInitMin) {
			raw_MinPoint = point;
			HasInitMin = true;
			return;
		}

		if (raw_MinPoint.x > point.x)
			raw_MinPoint.x = point.x;
		if (raw_MinPoint.y > point.y)
			raw_MinPoint.y = point.y;
		if (raw_MinPoint.z > point.z)
			raw_MinPoint.z = point.z;

	}

	public static void CheckSetRawMaxPoint(Coordinates point) {
		if (!HasInitMax) {
			raw_MaxPoint = point;
			HasInitMax = true;
			return;
		}

		if (raw_MaxPoint.x < point.x)
			raw_MaxPoint.x = point.x;
		if (raw_MaxPoint.y < point.y)
			raw_MaxPoint.y = point.y;
		if (raw_MaxPoint.z < point.z)
			raw_MaxPoint.z = point.z;

	}
}
