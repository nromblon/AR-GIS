using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore.Examples.ObjectManipulation;
using GoogleARCore.Examples.ObjectManipulationInternal;

/// <summary>
/// Basically a copy of ScaleManipulator but changed some attributes/methods
/// </summary>
public class ExtendedScaleManipulator : Manipulator
{

	/// <summary>
	/// The minimum scale of the object.
	/// </summary>
	public new float MinScale = 0.01f;
	
	/// <summary>
	/// The maximum scale of the object.
	/// </summary>
	public float MaxScale = 10f;

	private const float k_ElasticRatioLimit = 0.8f;
	private const float k_Sensitivity = 0.75f;
	private const float k_Elasticity = 0.15f;

	private float m_CurrentScaleRatio;
	private bool m_IsScaling;


	private float ScaleDelta {
		get {
			if (MinScale > MaxScale) {
				Debug.LogError("minScale must be smaller than maxScale.");
				return 0.0f;
			}

			return MaxScale - MinScale;
		}
	}

	private float ClampedScaleRatio {
		get {
			return Mathf.Clamp01(m_CurrentScaleRatio);
		}
	}

	private float CurrentScale {
		get {
			float elasticScaleRatio = ClampedScaleRatio + ElasticDelta();
			float elasticScale = MinScale + (elasticScaleRatio * ScaleDelta);
			return elasticScale;
		}
	}

	/// <summary>
	/// Enabled the scale controller.
	/// </summary>
	protected override void OnEnable() {
		base.OnEnable();
		m_CurrentScaleRatio = (transform.localScale.x - MinScale) / ScaleDelta;
	}

	/// <summary>
	/// Returns true if the manipulation can be started for the given gesture.
	/// </summary>
	/// <param name="gesture">The current gesture.</param>
	/// <returns>True if the manipulation can be started.</returns>
	protected override bool CanStartManipulationForGesture(PinchGesture gesture) {
		if (!IsSelected()) {
			return false;
		}

		//if (gesture.TargetObject != null) {
		//	return false;
		//}


		Debug.Log(gameObject.name + " - ExtendedScaleManipulator: Can Manipulate");

		return true;
	}
	
	/// <summary>
	/// Recalculates the current scale ratio in case local scale, min or max scale were changed.
	/// </summary>
	/// <param name="gesture">The gesture that started this transformation.</param>
	protected override void OnStartManipulation(PinchGesture gesture) {
		m_IsScaling = true;
		m_CurrentScaleRatio = (transform.localScale.x - MinScale) / ScaleDelta;
	}

	/// <summary>
	/// Continues the scaling of the object.
	/// </summary>
	/// <param name="gesture">The current gesture.</param>
	protected override void OnContinueManipulation(PinchGesture gesture) {
		m_CurrentScaleRatio +=
			k_Sensitivity * GestureTouchesUtility.PixelsToInches(gesture.GapDelta);

		float currentScale = CurrentScale;
		transform.localScale = new Vector3(currentScale, currentScale, currentScale);

		// If we've tried to scale too far beyond the limit, then cancel the gesture
		// to snap back within the scale range.
		if (m_CurrentScaleRatio < -k_ElasticRatioLimit
			|| m_CurrentScaleRatio > (1.0f + k_ElasticRatioLimit)) {
			gesture.Cancel();
		}
	}

	/// <summary>
	/// Finishes the scaling of the object.
	/// </summary>
	/// <param name="gesture">The current gesture.</param>
	protected override void OnEndManipulation(PinchGesture gesture) {
		m_IsScaling = false;
	}

	private float ElasticDelta() {
		float overRatio = 0.0f;
		if (m_CurrentScaleRatio > 1.0f) {
			overRatio = m_CurrentScaleRatio - 1.0f;
		}
		else if (m_CurrentScaleRatio < 0.0f) {
			overRatio = m_CurrentScaleRatio;
		}
		else {
			return 0.0f;
		}

		return (1.0f - (1.0f / ((Mathf.Abs(overRatio) * k_Elasticity) + 1.0f)))
		* Mathf.Sign(overRatio);
	}

	private void LateUpdate() {
		if (!m_IsScaling) {
			m_CurrentScaleRatio =
				Mathf.Lerp(m_CurrentScaleRatio, ClampedScaleRatio, Time.deltaTime * 8.0f);
			float currentScale = CurrentScale;
			transform.localScale = new Vector3(currentScale, currentScale, currentScale);
		}
	}

	public void SetMinMax(float newMin, float newMax) {
		SetMin(newMin);
		SetMax(newMax);
	}

	public void SetMin(float newMin) {
		MinScale = newMin;
	}

	public void SetMax(float newMax) {
		MaxScale = newMax;
	}
}
