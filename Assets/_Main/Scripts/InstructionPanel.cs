using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InstructionPanel : MonoBehaviour
{
	private static InstructionPanel instance;
	public static InstructionPanel Instance {
		get {
			return instance;
		}
	}

	[SerializeField] private Animator spriteAnimator;
	[SerializeField] private GameObject canvasGO;
	[SerializeField] private TextMeshProUGUI instructionTM;
	[SerializeField] private Button okButton;
	[SerializeField] private Text buttonTxt;

	private void Awake() {
		instance = this;
	}

	private void Start() {
		HidePanel();
	}

	public void HidePanel() {
		canvasGO.SetActive(false);
		spriteAnimator.SetTrigger("stop");
	}

	public void EnableButton(bool val) {
		if (val) {
			buttonTxt.text = "OK";
		}
		else {
			buttonTxt.text = "Wait...";
		}
		okButton.interactable = val;
	}

	public void ShowInstruction(string trigger, bool changeActive = false) {
		if (changeActive)
			spriteAnimator.SetTrigger("stop");

		canvasGO.SetActive(true);
		Debug.Log($"[InstructionPanel] instruction: [[{trigger}]]");
		switch (trigger) {
			case InstructionConstants.SCAN_TR:
				spriteAnimator.SetTrigger(trigger);
				instructionTM.text = InstructionConstants.SCAN_TXT;
				break;
			case InstructionConstants.HOST_TXT:
				spriteAnimator.SetTrigger(trigger);
				instructionTM.text = InstructionConstants.HOST_TXT;
				EnableButton(false);
				break;
			case InstructionConstants.ERROR_TR:
				spriteAnimator.SetTrigger(trigger);
				instructionTM.text = InstructionConstants.ERROR_TXT;
				break;
			case InstructionConstants.HOST_SUCCESS_TR:
				spriteAnimator.SetTrigger(trigger);
				instructionTM.text = InstructionConstants.HOST_SUCCESS_TXT;
				EnableButton(true);
				break;
			case InstructionConstants.SET_TR:
				spriteAnimator.SetTrigger(trigger);
				instructionTM.text = InstructionConstants.SET_TXT;
				break;
			default:
				HidePanel();
				break;
		}
	}
}

public class InstructionConstants {
	public const string SET_TR = "set";
	public const string SET_TXT = "Scan the area by moving the camera around the surface slowly. Tap on a plane when they are detected to place the play area.";

	public const string HOST_TR = "host";
	public const string HOST_TXT = "Attempting to host cloud anchor...";

	public const string SCAN_TR = "scan";
	public const string SCAN_TXT = "Move the camera around the target surface slowly to scan it.";

	public const string ERROR_TR = "error";
	public const string ERROR_TXT = "Anchor Cloud Service error occurred!";

	public const string HOST_SUCCESS_TR = "check";
	public const string HOST_SUCCESS_TXT = "Anchor Host Success!";
}