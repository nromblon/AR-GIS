using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore.Examples.ObjectManipulation;

public class PlayAreaScaleManipulator : Manipulator
{
	public Transform PointB;
	public bool m_IsScaling = false;

	protected override bool CanStartManipulationForGesture(DragGesture gesture) {
		return true;
	}

	protected override void OnStartManipulation(DragGesture gesture) {
		m_IsScaling = true;
	}

	protected override void OnContinueManipulation(DragGesture gesture) {
		Vector3 touchPos = Camera.main.ScreenToWorldPoint(gesture.Position);

		//Ray r Camera.main.ScreenPointToRay(gesture.Position);
		
		if (!IsTouchPosValid(touchPos))
			return;

		PerformScaleChange(touchPos);
	}

	protected override void OnEndManipulation(DragGesture gesture) {
		m_IsScaling = false;
	}

	protected override void OnSelected() {
		base.OnSelected();
	}

	private void PerformScaleChange(Vector3 touchPos) {
		var z_prev = Mathf.Abs(PointB.position.z - transform.position.z); 
		var x_prev = Mathf.Abs(PointB.position.x - transform.position.x);

		var z_prime = Mathf.Abs(touchPos.z - transform.position.z);
		var x_prime = Mathf.Abs(touchPos.x - transform.position.x);

		var z_ratio = z_prime / z_prev;
		var x_ratio = x_prime / x_prev;

		var x_scale = transform.localScale.x * x_ratio;
		var z_scale = transform.localScale.z * z_ratio;

		transform.localScale = new Vector3(x_scale, transform.localScale.y, z_scale);
	}

	private bool IsTouchPosValid(Vector3 touchPos) {

		return true;
	}
}
