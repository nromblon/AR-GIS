using UnityEngine;
using GoogleARCore;
using GoogleARCore.Examples.ObjectManipulationInternal;
using GoogleARCore.Examples.ObjectManipulation;

/// <summary>
/// Exactly the same as TranslationManipulator, but without the visualization requirement
/// </summary>
public class AnchorlessTranslationManipulator : Manipulator {
	// If specified, translates the rootObject instead.
	public GameObject rootObject;

	/// <summary>
	/// The translation mode of this object.
	/// </summary>
	public TransformationUtility.TranslationMode ObjectTranslationMode;

	/// <summary>
	/// The maximum translation distance of this object.
	/// </summary>
	public float MaxTranslationDistance;

	private const float k_PositionSpeed = 12.0f;
	private const float k_DiffThreshold = 0.0001f;

	private bool m_IsActive = false;
	private Vector3 m_PreviousWorldPoint;
	private Vector3 m_DesiredLocalPosition;

	/// <summary>
	/// The Unity's Start method.
	/// </summary>
	protected void Start() {
		m_DesiredLocalPosition = new Vector3(0, 0, 0);
		if (rootObject == null)
			rootObject = this.gameObject;
	}

	/// <summary>
	/// The Unity's Update method.
	/// </summary>
	protected override void Update() {
		base.Update();
		UpdatePosition();
	}

	/// <summary>
	/// Returns true if the manipulation can be started for the given gesture.
	/// </summary>
	/// <param name="gesture">The current gesture.</param>
	/// <returns>True if the manipulation can be started.</returns>
	protected override bool CanStartManipulationForGesture(DragGesture gesture) {
		if (!IsSelected())
			return false;

		Debug.Log(gameObject.name + " - NoVizTranslationManipulator: Can Manipulate");

		return true;
	}

	/// <summary>
	/// Function called when the manipulation is started.
	/// </summary>
	/// <param name="gesture">The current gesture.</param>
	protected override void OnStartManipulation(DragGesture gesture) {
		Ray r = Camera.main.ScreenPointToRay(gesture.Position);
		Plane hPlane = new Plane(Vector3.up, rootObject.transform.position);
		float distance;
		if (hPlane.Raycast(r, out distance)) {
			m_PreviousWorldPoint = r.GetPoint(distance);
		}
	}

	/// <summary>
	/// Continues the translation.
	/// </summary>
	/// <param name="gesture">The current gesture.</param>
	protected override void OnContinueManipulation(DragGesture gesture) {
		Vector2 currentGesturePos = gesture.Position;
		m_IsActive = true;

		Ray r_new = Camera.main.ScreenPointToRay(currentGesturePos);

		Plane hPlane = new Plane(Vector3.up, rootObject.transform.position);
		float distance;
		if (hPlane.Raycast(r_new, out distance)) {
			Vector3 worldPoint = r_new.GetPoint(distance);
			Vector3 direction = worldPoint - m_PreviousWorldPoint;
			Vector3 localDirection = rootObject.transform.parent.InverseTransformDirection(direction);
			// Remove y direction
			localDirection = new Vector3(localDirection.x, 0, localDirection.z);
			m_DesiredLocalPosition = rootObject.transform.localPosition + localDirection;

			m_PreviousWorldPoint = worldPoint;
		}
		else {
			Debug.LogError("Ray did not intersect with plane!");
		}
	}

	protected override void OnEndManipulation(DragGesture gesture) {
		//m_IsActive = false;
	}

	private void UpdatePosition() {
		if (!m_IsActive) {
			return;
		}

		// Lerp position.
		Vector3 oldLocalPosition = rootObject.transform.localPosition;
		Vector3 newLocalPosition = Vector3.Lerp(
			oldLocalPosition, m_DesiredLocalPosition, Time.deltaTime * k_PositionSpeed);

		//Vector3 newLocalPosition = m_DesiredLocalPosition;

		float diffLenght = (m_DesiredLocalPosition - newLocalPosition).magnitude;
		if (diffLenght < k_DiffThreshold) {
			newLocalPosition = m_DesiredLocalPosition;
			m_IsActive = false;
		}

		rootObject.transform.localPosition = newLocalPosition;
	}
}

