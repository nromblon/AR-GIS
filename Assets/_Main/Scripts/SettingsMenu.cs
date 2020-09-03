using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using GoogleARCore.Examples.Common;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
	private static SettingsMenu sharedInstance;
	public static SettingsMenu Instance {
		get { return sharedInstance; }
	}

	[Header("Setup")]
	[SerializeField] private DepthSettings m_DepthSettings;
	/// <summary>
	/// The button to open the menu window.
	/// </summary>
	[SerializeField] private Button m_MenuButton = null;
	/// <summary>
	/// The Menu Window shows the depth configurations.
	/// </summary>
	[SerializeField] private GameObject m_MenuWindow = null;
	[SerializeField] private Button m_ResetCityButton = null;
	[SerializeField] private Button m_OpenDepthSettingsButton = null;
	[SerializeField] private Button m_ReturnButton = null;

	private ARSceneController sceneController;

	private void Awake() {
		sharedInstance = this;
	}

	// Start is called before the first frame update
	void Start()
    {
		sceneController = ARSceneController.Instance;
		m_MenuButton.onClick.AddListener(() => SetMenuWindow(true));
		m_ResetCityButton.onClick.AddListener(_OnResetCityButtonClicked);
		m_OpenDepthSettingsButton.onClick.AddListener(_OnDepthSettingsButtonClicked);
		m_ReturnButton.onClick.AddListener(_OnReturnButtonClicked);
    }

	public void SetMenuWindow(bool val) {
		m_MenuWindow.SetActive(val);
	}

	public void EnableResetCityPlacement(bool val) {
		Debug.Log("Reset City Placement enabled:" + val);
		m_ResetCityButton.interactable = val;
		Debug.Log("ResetButton status: " + m_ResetCityButton.interactable);
	}

	private void _OnResetCityButtonClicked() {
		EnableResetCityPlacement(false);
		sceneController.ResetPlayAreaPlacement();
		_OnReturnButtonClicked();
	}

	private void _OnDepthSettingsButtonClicked() {
		SetMenuWindow(false);
		m_DepthSettings._OnMenuButtonClicked();
	}

	private void _OnReturnButtonClicked() {
		SetMenuWindow(false);
		sceneController.EnablePlaneDiscoveryGuide(sceneController.IsPlaneDiscoveryGuideActive);
	}
}
