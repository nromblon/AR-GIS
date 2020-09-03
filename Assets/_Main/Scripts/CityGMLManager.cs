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

public class CityGMLManager : MonoBehaviour
{
	private static CityGMLManager sharedInstance;
	public static CityGMLManager Instance {
		get {
			return sharedInstance;
		}
	}

	[Header("Config")]
	public float y_offset = -5;
	public CityGml2GO cityGML2GO;
	public GenerateColliderOptions GenerateColliders = 0;
	public bool shouldRemoveOutliers = true;
	public float outlierCoeff = 3f;

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
		b_IsCityPlaced = false;

    }

	#region Initialization Methods
	public void InitializeCity() {
		// Recenter first level children
		RecenterChildren();
		
		// recenter buildings
		RecenterBuildings();

		// Generate Colliders
		GenerateBuildingColliders();

		onLoadPosition = transform.position;
		onLoadScale = transform.localScale;
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
		Debug.Log("Center located at: " + center);

		// Detach 1st level children
		foreach (var child in children) {
			child.parent = null;
		}

		transform.position = new Vector3(center.x, transform.position.y, center.z);

		// Reattach children
		Debug.Log("Reattaching");
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
	#endregion

	public void CheckWithinBounds(BoxCollider box) {
		if(buildings == null) {
			buildings = GetComponentsInChildren<BuildingProperties>();
		}

		foreach(var building in buildings) {
			if (Utilities.IsPointWithinBoxCollider(building.transform.position, box)) {
				//TODO: optimization opportunity. Should I check if active or is it fine as is?
				// might want to check profiler for results.
				//building.gameObject.SetActive(true);
			}
			else {
				//building.gameObject.SetActive(false);
			}
		}
	}

	public void OnCityPlaced() {
		b_IsCityPlaced = true;

		var scaleManipulator = GetComponent<ExtendedScaleManipulator>();
		var curScale = transform.localScale.x;

		scaleManipulator.SetMinMax(curScale, curScale * 8);
		Select();
		originOnPlaced = transform.localPosition;
	}

	public void OnCityRemoved() {
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
			Debug.Log("city manipulator selected");
			m.enabled = true;
			m.Select();
		}
	}

	public void Deselect() {
		foreach (var m in Manipulators) {
			m.Deselect();
			m.enabled = false;

			Debug.Log("CityManipulators deselected");
		}
	}

	public void ResetPositionToOrigin(float duration) {
		StartCoroutine(LerpPositionToOrigin(duration));
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
}
