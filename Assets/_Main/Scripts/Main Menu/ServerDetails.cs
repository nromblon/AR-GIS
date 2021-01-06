using FixCityAR;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ServerDetails : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI serverNameTM;
	[SerializeField] private TextMeshProUGUI hostNameTM;
	[SerializeField] private TextMeshProUGUI numUsersTM;
	[SerializeField] private TextMeshProUGUI loadedFilesTM;
	[SerializeField] private TextMeshProUGUI[] hideableTM;
	[SerializeField] private Scrollbar scrollbar;

	const string hostNamePrefix = "by ";
	const string numUsersPrefix = "Connected Users: ";

	public void SetDetails(DiscoveryResponse info) {
		if (info == null) {
			Debug.Log("[ServerDetails] Info parameter is null. Clearing details");
			serverNameTM.text = "";
			hostNameTM.text = "";
			numUsersTM.text = "";
			loadedFilesTM.text = "";

			foreach (var tm in hideableTM) {
				tm.gameObject.SetActive(false);
			}

			scrollbar.interactable = false;
		}
		else {
			Debug.Log("[ServerDetails] Setting ");
			serverNameTM.text = info.serverName;
			hostNameTM.text = hostNamePrefix + info.hostUsername;
			numUsersTM.text = numUsersPrefix + info.numUsers + "/" + info.maxUsers;
			string filesStr = "";
			foreach (string file in info.loadedFiles) {
				filesStr += file + "\n";
			}
			loadedFilesTM.text = filesStr;

			foreach (var tm in hideableTM) {
				tm.gameObject.SetActive(true);
			}

			scrollbar.interactable = true;
		}
	}

}
