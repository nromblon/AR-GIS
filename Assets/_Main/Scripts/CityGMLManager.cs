using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.CityGML2GO;
using FixCityAR;
using GoogleARCore.Examples.ObjectManipulation;
using Assets.Scripts.CityGML2GO.Scripts;

public class CityGMLManager : MonoBehaviour
{
	private static CityGMLManager sharedInstance;
	public static CityGMLManager Instance {
		get {
			return sharedInstance;
		}
	}

	public CityGml2GO cityGML2GO;
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

	public Manipulator[] Manipulators;
	public bool b_IsCityPlaced;
	private BuildingProperties[] buildings;

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
		this.cityGML2GO = GetComponent<CityGml2GO>();
		b_IsCityPlaced = false;

    }

	public void RecenterChildren() {
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
		//reset position
		transform.position = Vector3.zero;
	}


	public void CheckWithinBounds(Bounds b) {
		if(buildings == null) {
			buildings = GetComponentsInChildren<BuildingProperties>();
		}

		foreach(var building in buildings) {
			if (b.Contains(building.transform.position)) {
				//TODO: optimization opportunity. Should I check if active or is it fine as is?
				// might want to check profiler for results.
				building.gameObject.SetActive(true);
			}
			else {
				building.gameObject.SetActive(false);
			}
		}
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
}
