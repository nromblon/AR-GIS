using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore.Examples.ObjectManipulation;

public class CityTranslationManipulator : TranslationManipulator
{

    // Update is called once per frame
    protected override void Update()
    {
		base.Update();
		//if (CityManager.Instance.b_IsCityPlaced) {
		//	BoxCollider collider = PlayAreaManager.Instance.PlayArea.MeshBoundary.GetComponent<BoxCollider>();
		//	//Debug.Log("PlayArea Collider bounds min: " + collider.bounds.min + " - max: " + collider.bounds.max);
		//	CityManager.Instance.CheckWithinBounds(collider);
		//}
	}

	/// <summary>
	/// Returns true if the manipulation can be started for the given gesture.
	/// </summary>
	/// <param name="gesture">The current gesture.</param>
	/// <returns>True if the manipulation can be started.</returns>
	protected override bool CanStartManipulationForGesture(DragGesture gesture) {
		if (gesture.TargetObject == null) {
			return false;
		}

		// If the gesture isn't targeting this item, don't start manipulating.
		if (gesture.TargetObject != gameObject) {
			return false;
		}

		Debug.Log(gameObject.name + " - CityTranslationManipulator: Can Manipulate");

		return true;
	}
}
