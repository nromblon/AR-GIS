using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownController : MonoBehaviour
{
	public Transform ScaleTf;

	public float MinScale = 0.3f, MaxScale = 8f;
	public float ScaleFactor = .002f;


	/// <summary>
	/// Code by Lukasz Motyczka, submitted on 21 March, 2016
	/// From: https://stackoverflow.com/questions/36129929/how-to-scale-in-and-out-objects-individual-with-pinch-zoom
	/// </summary>
	/// <param name="touch0"></param>
	/// <param name="touch1"></param>
	public void PinchToScale(Touch touch0, Touch touch1) {
		// Store both touches.

		// Find the position in the previous frame of each touch.
		Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
		Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

		// Find the magnitude of the vector (the distance) between the touches in each frame.
		float prevTouchDeltaMag = (touch0PrevPos - touch1PrevPos).magnitude;
		float touchDeltaMag = (touch0.position - touch1.position).magnitude;

		// Find the ratio in the distances between each frame.
		float deltaMagnitudeRatio = touchDeltaMag - prevTouchDeltaMag;

		Debug.Log("Delta Magnitude Ratio: " + deltaMagnitudeRatio);

		IncreaseScale(deltaMagnitudeRatio);

	}

	public void ScaleTown(float newScale) {
		ScaleTf.localScale = new Vector3(newScale, newScale, newScale);
	}

	public void IncreaseScale(float increaseBy) {
		float currentScale = ScaleTf.localScale.x;
		increaseBy = ScaleFactor * increaseBy;

		float newScale = Mathf.Clamp(currentScale + increaseBy, MinScale, MaxScale);

		Debug.Log("New scale: " + newScale);

		ScaleTf.localScale = new Vector3(newScale, newScale, newScale);
	}
}
