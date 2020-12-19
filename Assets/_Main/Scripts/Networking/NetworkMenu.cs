using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FixCityAR {
	public class NetworkMenu : MonoBehaviour {

		[SerializeField] private GameObject hostRoomGroup;
		[SerializeField] private GameObject joinRoomGroup;

		[SerializeField] private TMP_InputField username;
		[SerializeField] private TMP_InputField roomname;
		[SerializeField] private TMP_InputField password;

		[SerializeField] private Button hostButton;
		[SerializeField] private Button showServersButton;

		[SerializeField] private Button proceedJoin;
		[SerializeField] private ServerList roomList;

		[SerializeField] private DiscoveryHUDController discoveryController; 

		private bool isRoomListShown = false;

		public void AddServer(DiscoveryResponse info) {
			roomList.AddItem(info);
		}

		public void HostSession() {
			string hostName = this.roomname.text;
			string username = this.username.text;
			discoveryController.StartHost(hostName, username);
		}

		public void ShowSessionList() {
			ToggleRoomListShown();
			discoveryController.FindServers();
		}

		public void RefreshSessionList() {
			roomList.ClearList();
			discoveryController.FindServers();
		}

		public void CheckIfButtonsEnable() {
			if (!username.text.Equals("")) {
				if(roomList.selectedItem != null) {
					proceedJoin.interactable = true;
				}
				else {
					proceedJoin.interactable = false;
				}

				if (!roomname.text.Equals("")) {
					hostButton.interactable = true;
				}
				else {
					hostButton.interactable = false;
				}
			}
			else {
				proceedJoin.interactable = false;
				hostButton.interactable = false;
			}
		}

		public void JoinSession() {
			
		}

		public void ShowMenu() {
			gameObject.SetActive(true);
		}

		public void HideMenu() {
			ClearInputFields();
			gameObject.SetActive(false);
			discoveryController.StopFinding();
			roomList.ClearList();
		}

		private void ClearInputFields() {
			roomname.text = "";
			password.text = "";
		}

		public void ToggleRoomListShown() {
			if (isRoomListShown) {
				joinRoomGroup.SetActive(false);
				hostRoomGroup.SetActive(true);
				isRoomListShown = false;
			}
			else {
				joinRoomGroup.SetActive(true);
				hostRoomGroup.SetActive(false);
				isRoomListShown = true;
			}
		}

		public void BackButtonPressed() {
			if (isRoomListShown) {
				ToggleRoomListShown();
			}
			else {
				HideMenu();
			}
		}
	}
}