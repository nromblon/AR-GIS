using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlsManager : MonoBehaviour
{
	private static ControlsManager sharedInstance;
	public static ControlsManager Instance {
		get { return sharedInstance; }
	}

	const string SHOW_BUTTONS = "IsShown";

	[Header("Setup")]
	[SerializeField] private Animator ControlsAnimator;
	[SerializeField] private Button m_recenterButton;
	[SerializeField] private Image m_recenterIcon;

	[Header("Config")]
	public float resetDuration = 1.4f;
	[SerializeField] private Color disabledIconColor;

	private ARSceneController sceneController;
	private CityManager cityManager;
	private Color defaultIconColor;

	private void Awake() {
		sharedInstance = this;
	}

	// Start is called before the first frame update
	void Start()
    {
		sceneController = ARSceneController.Instance;
		cityManager = CityManager.Instance;
		defaultIconColor = m_recenterIcon.color;
    }

    public void ResetCityPosition() {
		cityManager.ResetPositionToOrigin(resetDuration);
		EnableRecenterButton(false);
	}

	public void ShowControls(bool val) {
		ControlsAnimator.SetBool(SHOW_BUTTONS, val);
		Debug.Log("Controls Shown: " + val);
	}

	public void EnableRecenterButton(bool val) {
		if (val) {
			m_recenterButton.interactable = true;
			m_recenterIcon.color = defaultIconColor;
		}
		else {
			m_recenterButton.interactable = false;
			m_recenterIcon.color = disabledIconColor;
		}
	}
}
