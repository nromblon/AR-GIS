using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.CityGML2GO;
using FixCityAR;

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


	private void Awake() {
		sharedInstance = this;
	}

	// Start is called before the first frame update
	void Start()
    {
		this.cityGML2GO = GetComponent<CityGml2GO>();
    }

	public void RecenterChildren() {
		var children = transform.GetComponentsInChildren<Transform>();
		var center = Bounds.center;
		Debug.Log("Center located at: " + center);
		transform.DetachChildren();
		transform.position = new Vector3(center.x, transform.position.y, center.z);
		// Reattach children
		Debug.Log("Reattaching");
		foreach(var child in children) {
			child.SetParent(transform);
		}
		//reset position
		transform.position = Vector3.zero;
	}
}
