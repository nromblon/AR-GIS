using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClippingPlane : MonoBehaviour {
	//material we pass the values to
	public Transform[] planes;
	public Material[] mats;

	//execute every frame
	void Update() {
		//create plane
		for (int i = 0; i < 4; i++) { 
			Plane plane = new Plane(planes[i].up, planes[i].position);
			//transfer values from plane to vector4
			Vector4 planeRepresentation = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);
			//pass vector to shader
			foreach (var mat in mats) {
				mat.SetVector("_Plane" + i, planeRepresentation);
			}
		}
	}
}