using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
	[SerializeField] public Button button;
	[SerializeField] private Image buttonIcon;
	[SerializeField] private Image buttonBg;

	public void SetIconColor(Color color) {
		buttonIcon.color = color;
	}

	public void SetBGColor(Color color) {
		buttonBg.color = color;
	}

	public void SetColor(Color iconColor, Color bgColor) {
		SetIconColor(iconColor);
		SetBGColor(bgColor);
	}
}
