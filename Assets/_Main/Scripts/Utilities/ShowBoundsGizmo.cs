using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixCityAR;

public enum GizmoDrawFrom {
	GetBoundsUtility = 0,
	VectorZero = 1,
	BoxCollider = 2
}

/// <summary>
/// Debugging tool to visualize the renderer bounds (but as a cube) of the gameobject and its children
/// </summary>
public class ShowBoundsGizmo : MonoBehaviour
{
	public Color color = Color.yellow;
	public GameObject min, max, center;

	public GizmoDrawFrom drawFrom = 0;

	private void OnDrawGizmos() {
		Gizmos.color = this.color;
		Matrix4x4 oldMatrix = Gizmos.matrix;
		//Gizmos.matrix = transform.localToWorldMatrix;
		switch (drawFrom) {
			case (GizmoDrawFrom.GetBoundsUtility):
				var bounds = Utilities.GetBounds(gameObject);
				Gizmos.DrawWireCube(bounds.center, bounds.size);

				if (min != null && max != null) {
					min.transform.position = bounds.min;
					max.transform.position = bounds.max;
				}

				if (center != null) {
					center.transform.position = bounds.center;
				}
				break;

			case (GizmoDrawFrom.VectorZero):
				Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
				break;

			case (GizmoDrawFrom.BoxCollider):
				bounds = GetComponent<BoxCollider>().bounds;
				Gizmos.DrawWireCube(bounds.center, bounds.size);

				if (min != null && max != null) {
					min.transform.position = bounds.min;
					max.transform.position = bounds.max;
				}

				if (center != null) {
					center.transform.position = bounds.center;
				}
				break;
		}
		Gizmos.matrix = oldMatrix;
	}
	
}
