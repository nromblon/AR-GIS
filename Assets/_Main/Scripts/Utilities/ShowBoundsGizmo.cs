using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixCityAR;

/// <summary>
/// Debugging tool to visualize the renderer bounds (but as a cube) of the gameobject and its children
/// </summary>
public class ShowBoundsGizmo : MonoBehaviour
{
	public Color color = Color.yellow;
	public GameObject min, max, center;

	private void OnDrawGizmos() {
		Gizmos.color = this.color;
		Matrix4x4 oldMatrix = Gizmos.matrix;
		Gizmos.matrix = transform.localToWorldMatrix;
		var bounds = Utilities.GetBounds(gameObject);
		Gizmos.DrawWireCube(bounds.center, bounds.size);

		Gizmos.matrix = oldMatrix;

		if(min != null && max != null) {
			min.transform.position = bounds.min;
			max.transform.position = bounds.max;
		}

		if(center != null) {
			center.transform.position = bounds.center;
		}
	}
}
