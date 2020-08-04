using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertPin : MonoBehaviour
{
	public Canvas Canvas;
	//private Animator CanvasAnimator;

	private bool isCanvasShown = false;

	public void Awake() {
		//CanvasAnimator = Canvas.GetComponent<Animator>();

		Canvas.worldCamera = GameObject.FindGameObjectWithTag("UI Camera").GetComponent<Camera>();
		Debug.Log("Canvas Camera: " + Canvas.worldCamera.name);
	}

	public bool ToggleCanvas() {
		if (!isCanvasShown) {
			// Show the UI
			//CancelInvoke();
			Canvas.gameObject.SetActive(true);
			//CanvasAnimator.SetTrigger("Show");
			isCanvasShown = true;
			Debug.Log("In Alert Pin: Shown");
		}
		else {
			// Hide the UI
			//CanvasAnimator.SetTrigger("Hide");
			isCanvasShown = false;
			//Invoke("_DisableCanvas", .1f);
			Canvas.gameObject.SetActive(false);
			Debug.Log("In Alert Pin: Hidden");
		}

		return isCanvasShown;
	}

	public void DisableCanvas() {
		if (!isCanvasShown)
			return;

		//CanvasAnimator.SetTrigger("Hide");
		isCanvasShown = false;
		Invoke("_DisableCanvas", .1f);
	}

	private void _DisableCanvas() {
		Canvas.gameObject.SetActive(false);
	}
}
