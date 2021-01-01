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
		[SerializeField] private ServerList serverList;

		[SerializeField] private NewNetworkDiscovery netDiscovery;

		private bool isRoomListShown = false;

		private NewNetworkManager netManager;

		private void Start() {
			netManager = (NewNetworkManager) NewNetworkManager.singleton;
		}

		#region Main Networking Buttons
		public void HostSession() {
			string hostName = this.roomname.text;
			string username = this.username.text;
			netDiscovery.StopDiscovery();

			PlayerPrefs.SetString("username", username);
			netManager.roomName = roomname.text;
			netManager.loadedFiles = CityManager.Instance.cityGML2GO.LoadedFiles;
			netManager.StartHost();
			netDiscovery.AdvertiseServer();
			Debug.Log("Hosting Session");
		}

		public void JoinSession() {
			PlayerPrefs.SetString("username", username.text);
			netManager.StartClient(serverList.selectedItem.Info.uri);
			netDiscovery.StopDiscovery();
			//discoveryController.Connect(serverList.selectedItem.Info);
			Debug.Log("Joining Session");
		}
		#endregion

		#region Discovery
		/// <summary>
		/// Listener for discovered server. Set in Editor, under NetworkDiscovery script.
		/// </summary>
		/// <param name="info"></param>
		public void OnDiscoveredServer(DiscoveryResponse info) {
			Debug.Log($"Discovered Server: {info.serverName} by {info.hostUsername} (IP: {info.EndPoint.Address.ToString()})");
			AddServer(info);
		}

		private void AddServer(DiscoveryResponse info) {
			serverList.AddItem(info);
		}

		public void ShowSessionList() {
			ToggleRoomListShown();
			netDiscovery.StartDiscovery();
		}

		public void RefreshSessionList() {
			netDiscovery.StopDiscovery();
			serverList.ClearList();
			netDiscovery.StartDiscovery();
			proceedJoin.interactable = false;
		}
		#endregion

		#region UI and Utility functions
		public void ShowMenu() {
			gameObject.SetActive(true);
		}

		public void HideMenu() {
			ClearInputFields();
			gameObject.SetActive(false);

			netDiscovery.StopDiscovery();
			serverList.ClearList();
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

		public void CheckIfButtonsEnable() {
			if (!username.text.Equals("")) {
				if (serverList.selectedItem != null) {
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
		#endregion

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