using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public enum ARTool {
	Hand = 0, InfoPin = 1, Ping = 2
}

public class ControlsManager : MonoBehaviour
{
	private static ControlsManager sharedInstance;
	public static ControlsManager Instance {
		get { return sharedInstance; }
	}

	const string SHOW_BUTTONS = "IsShown";
	const string SHOW_CATALOG = "toolCatalogShown";

	[Header("Setup")]
	[SerializeField] private Animator ControlsAnimator;
	[SerializeField] private Button m_recenterButton;
	[SerializeField] private Image m_recenterIcon;

	[Header("Config")]
	public float resetDuration = 1.4f;
	[SerializeField] private Color disabledIconColor;

	[Header("Tool Catalog Setup")]
	[SerializeField] private GameObject toolCatalogParent;
	[SerializeField] private GameObject[] selectedToolIcons;
	/// <summary>
	/// Children order in array must be the same as enum
	/// </summary>
	[SerializeField] private ButtonController[] CatalogToolsController;
	[Header("Tool Catalog Config")]
	[SerializeField] private Color tool_selectedIconBg;
	[SerializeField] private Color tool_selectedIconColor;
	[SerializeField] private Color tool_unselectedIconBg;
	[SerializeField] private Color tool_unselectedIconColor;


	private ARSceneController sceneController;
	private CityManager cityManager;

	private Color selectedIconColor;
	private bool isCatalogOpen;
	
	public ARTool SelectedTool {
		get; private set;
	}

	private void Awake() {
		sharedInstance = this;
	}

	// Start is called before the first frame update
	void Start()
    {
		sceneController = ARSceneController.Instance;
		cityManager = CityManager.Instance;
		selectedIconColor = m_recenterIcon.color;
		isCatalogOpen = false;
		OnToolCatalogSelected("Hand");
    }

    public void ResetCityPosition() {
		cityManager.ResetPositionToOrigin(resetDuration);
		EnableRecenterButton(false);
	}

	public void ShowControls(bool val) {
		ControlsAnimator.SetBool(SHOW_BUTTONS, val);
	}

	public void ToggleOpenCatalog() {
		if (isCatalogOpen) {
			// Hide Catalog
			toolCatalogParent.SetActive(false);
		}
		else {
			// Show Catalog
			toolCatalogParent.SetActive(true);
			ControlsAnimator.SetBool(SHOW_CATALOG, true);
		}

		isCatalogOpen = !isCatalogOpen;
	}

	#region Tool Catalog Animation Callbacks
	public void OnToolAnimHidden() {
		for (int i = 0; i < CatalogToolsController.Length; i++) {
			CatalogToolsController[i].button.interactable = false;
		}
		toolCatalogParent.SetActive(false);
	}

	public void OnToolAnimShown() {
		for (int i = 0; i < CatalogToolsController.Length; i++) {
			if (i == (int)SelectedTool)
				continue;
			CatalogToolsController[i].button.interactable = true;
		}
	}
	#endregion

	public void OnToolCatalogSelected(string toolname) {
		ARTool selected;
		switch (toolname) {
			case "Hand": selected = ARTool.Hand;
				break;
			case "InfoPin":
				selected = ARTool.InfoPin;
				break;
			case "Ping":
				selected = ARTool.Ping;
				break;
			default:
				selected = ARTool.Hand;
				break;
		}

		if (selected == SelectedTool)
			return;

		ButtonController prevSelected = CatalogToolsController[(int)SelectedTool];
		ButtonController newSelected = CatalogToolsController[(int)selected];
		// Change interactable of selected tool from catalog
		prevSelected.button.interactable = true;
		newSelected.button.interactable = false;
		// Change color
		prevSelected.SetColor(tool_unselectedIconColor, tool_unselectedIconBg);
		newSelected.SetColor(tool_selectedIconColor, tool_selectedIconBg);
		// Change icon visibiliy on Selected Tool Btn
		selectedToolIcons[(int)SelectedTool].SetActive(false);
		selectedToolIcons[(int)selected].SetActive(true);

		SelectedTool = selected;
	}

	public void EnableRecenterButton(bool val) {
		if (val) {
			m_recenterButton.interactable = true;
			m_recenterIcon.color = selectedIconColor;
		}
		else {
			m_recenterButton.interactable = false;
			m_recenterIcon.color = disabledIconColor;
		}
	}
}
