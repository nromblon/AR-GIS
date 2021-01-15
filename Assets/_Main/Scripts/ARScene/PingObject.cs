using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class PingObject : PinnableObject
{
	[SerializeField] private bool alwaysFaceCamera = true;
	[SerializeField] private TextMeshProUGUI usernameText;
	[SerializeField] private Image eyeIcon;
	[SerializeField] private Canvas canvas;

	[SyncVar(hook = nameof(UsernameHook))]
	public string username;
	[SyncVar(hook = nameof(SetEyeColor))]
	public Color color;

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

	public void SetEyeColor(Color oldColor, Color newColor) {
		eyeIcon.color = newColor;
	}

	public void UsernameHook(string oldUsername, string newUsername) {
		usernameText.text = newUsername;
	}

	public override void ShowVisuals(bool val) {
		canvas.gameObject.SetActive(val);
	}
}
