using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FixCityAR;
using UnityEngine.UI;

public class ServerListItem : MonoBehaviour {
	[Header("Config")]
	[SerializeField] Color unselectedState;
	[SerializeField] Color selectedState;
	[SerializeField] Color unselectedStateTxt;
	[SerializeField] Color selectedStateTxt;


	private ServerList list;

	[Header("Setup")]
	[SerializeField] private TextMeshProUGUI text;
	[SerializeField] private Image btnImg;

	private DiscoveryResponse info;
	public DiscoveryResponse Info {
		get;
	}

	private void Start() {
		list = GetComponentInParent<ServerList>();
		btnImg = GetComponent<Image>();
	}

	public void SetServerInfo(DiscoveryResponse info) {
		this.info = info;

		this.text.text = info.serverName;
	}

	public void OnSelected() {
		list.OnItemSelected(info.serverId);
	}

	public void SetSelected(bool val) {
		if (val) {
			btnImg.color = selectedState;
			text.color = selectedStateTxt;
		}
		else {
			btnImg.color = unselectedState;
			text.color = unselectedStateTxt;
		}
	}
}
