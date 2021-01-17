using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoPin : PinnableObject
{
	[Header("Setup")]
	[SerializeField] private Canvas canvas;
	[SerializeField] private MeshRenderer renderer;
	[SerializeField] private TextMeshProUGUI usernameTM;
	[SerializeField] private TMP_InputField inputField;
	[SerializeField] private Button deleteBtn;

	private bool canvasPrevState = false;

	[SyncVar (hook=nameof(UsernameHook))] private string username;
	[SyncVar (hook=nameof(InfoHook))] private string pinText;

	public override void OnStartAuthority() {
		base.OnStartAuthority();
		ShowCanvas(true);
		inputField.interactable = true;
		deleteBtn.gameObject.SetActive(true);

		inputField.Select();
	}

	public override void OnStartClient() {
		base.OnStartClient();
		usernameTM.text = username;
		if (hasAuthority) {
			ShowCanvas(true);
			deleteBtn.gameObject.SetActive(true);
		}
		else {
			ShowCanvas(false);
			deleteBtn.gameObject.SetActive(false);
			inputField.interactable = false;
			inputField.text = pinText;
		}
	}

	public void ToggleShowCanvas() {
		ShowCanvas(!canvas.gameObject.activeSelf);
	}

	public void ShowCanvas(bool val) {
		canvas.gameObject.SetActive(val);
		canvasPrevState = val;
	}

	public void SetUsername(string username) {
		this.username = $"by {username}";
		//usernameTM.text = $"by {username}";
	}

	public void OnPinInfoSet() {
		this.pinText = inputField.text;
		CmdUpdatePinInfo(this.username, this.pinText);
	}
	
	void UsernameHook(string oldUsername, string newUsername) {
		usernameTM.text = newUsername;
	}

	void InfoHook(string oldInfo, string newInfo) {
		inputField.text = newInfo;
	}

	[Command]
	private void CmdUpdatePinInfo(string username, string infoTxt) {
		RpcUpdatePinInfo(username, infoTxt);
	}

	[ClientRpc]
	private void RpcUpdatePinInfo(string username, string infoTxt) {
		inputField.text = infoTxt;
		usernameTM.text = username;
		Debug.Log($"[InfoPin] Pin Parent: {transform.parent.name}");
	}

	public override void ShowVisuals(bool val) {
		renderer.enabled = val;
		if (!val)
			canvas.gameObject.SetActive(false);
		else
			canvas.gameObject.SetActive(canvasPrevState);
	}

	public override void PinToCity(Vector3 localPos) {
		base.PinToCity(localPos);
		transform.localScale = new Vector3(.5f, .5f, .5f);
	}
}
