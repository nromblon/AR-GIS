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

    // Start is called before the first frame update
    protected new void Start()
    {
		base.Start();
		inputField.interactable = false;
		deleteBtn.gameObject.SetActive(false);
		ShowCanvas(false);
	}

    // Update is called once per frame
    protected new void Update()
    {
		base.Update();
    }

	public override void OnStartAuthority() {
		base.OnStartAuthority();
		ShowCanvas(true);
		inputField.interactable = true;
		deleteBtn.gameObject.SetActive(true);

		inputField.Select();
	}

	public void ToggleShowCanvas() {
		ShowCanvas(!canvas.gameObject.activeSelf);
	}

	public void ShowCanvas(bool val) {
		canvas.gameObject.SetActive(val);
		canvasPrevState = val;
	}

	public void SetUsername(string username) {
		usernameTM.text = $"by {username}";
	}

	public void OnPinInfoSet() {
		CmdUpdatePinInfo(usernameTM.text, inputField.text);
	}
	
	[Command]
	private void CmdUpdatePinInfo(string username, string infoTxt) {
		RpcUpdatePinInfo(username, infoTxt);
	}

	[ClientRpc(excludeOwner = true)]
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
}
