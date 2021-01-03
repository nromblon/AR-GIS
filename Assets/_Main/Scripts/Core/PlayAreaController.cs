using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore.Examples.ObjectManipulation;
using FixCityAR;

public class PlayAreaController : MonoBehaviour
{
	const string SHOW_FLAG = "IsShown";

	public GameObject MeshBoundary;
	public GameObject MeshTf;
	public Manipulator[] Manipulators;

	private Bounds playAreaBounds;
	private MeshRenderer boxMesh;
	private Animator animator;

	// Returns the bounds of the object local-wise. Bounds are taken from collider.
	public Bounds Bounds {
		get {
			var center = GetComponentInChildren<BoxCollider>().center;
			var size = Vector3.Scale(GetComponentInChildren<BoxCollider>().size,
				MeshBoundary.transform.localScale);
			size = Vector3.Scale(size,MeshTf.transform.localScale);
			playAreaBounds = new Bounds(center, size);
			return playAreaBounds;
		}
	}
	
    void Awake()
    {
		Manipulators = MeshTf.GetComponents<Manipulator>();
		boxMesh = MeshBoundary.GetComponent<MeshRenderer>();
		animator = GetComponent<Animator>();
    }

	public void Select() {
		foreach (var m in Manipulators) {
			m.enabled = true;
			m.Select();
		}
	}

	public void OnPlacementConfirm() {
		animator.SetBool(SHOW_FLAG, false);
	}

	public void OnShowPlayArea() {
		animator.SetBool(SHOW_FLAG, true);
	}
}
