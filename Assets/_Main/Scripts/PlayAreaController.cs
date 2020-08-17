using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore.Examples.ObjectManipulation;
using FixCityAR;

public class PlayAreaController : MonoBehaviour
{
	public GameObject MeshBoundary;
	public Manipulator[] Manipulators;
	private Bounds playAreaBounds;
	// Returns the bounds of the object local-wise. Bounds are taken from collider.
	public Bounds Bounds {
		get {
			var center = GetComponentInChildren<BoxCollider>().center;
			var size = Vector3.Scale(GetComponentInChildren<BoxCollider>().size,
				MeshBoundary.transform.localScale);
			size = Vector3.Scale(size,transform.localScale);
			playAreaBounds = new Bounds(center, size);
			return playAreaBounds;
		}
	}

    // Start is called before the first frame update
    void Start()
    {
		Manipulators = GetComponents<Manipulator>();
    }

	public void Select() {
		foreach(var m in Manipulators) {
			m.Select();
		}
	}

	public void Deselect() {
		foreach(var m in Manipulators) {
			m.Deselect();
		}
	}

	void OnDestroy() {
		Deselect();
	}

	public void SetAsChild(GameObject go) {
		go.transform.SetParent(MeshBoundary.transform);
	}
}
