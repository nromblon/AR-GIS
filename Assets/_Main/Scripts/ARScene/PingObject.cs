using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PingObject : PinnableObject
{
	[SerializeField] private bool alwaysFaceCamera = true;
	[SerializeField] private Image eyeIcon;

	private Camera mainCamera;
    // Start is called before the first frame update
    protected new void Start()
    {
		base.Start();
		mainCamera = Camera.main;

	}

    // Update is called once per frame
    protected new void Update()
    {
		base.Update();
		transform.LookAt(mainCamera.transform);
    }

	public void SetEyeColor(Color color) {
		eyeIcon.color = color;
	}

}
