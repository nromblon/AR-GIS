using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CloudAnchorMessage : NetworkMessage {
	public string CloudId;
}

public class ARUser : NetworkBehaviour {

	[SyncVar]
	public string username;
	[SyncVar]
	public bool isHost;

	public ChatBehaviour ChatBehaviour { private set; get; }

	public CityTransformBehaviour CityTfBehaviour { private set; get; }

	// Start is called before the first frame update
	void Start() {
		transform.position = Vector3.zero;

		ChatBehaviour = GetComponent<ChatBehaviour>();
		CityTfBehaviour = GetComponent<CityTransformBehaviour>();

		if (hasAuthority) {
			username = PlayerPrefs.GetString("username");
			Debug.Log($"[ARUser] Setting username: {username}");

			PlayAreaManager.Instance.localUser = this;
		}

		isHost = isServer;
		Debug.Log($"[ARUser] Am I host?: {isHost}");
		if (isHost) {
			ARSceneController.Instance.applicationMode = ApplicationMode.Host;
		}
		else {
			ARSceneController.Instance.applicationMode = ApplicationMode.Client;
			// Only clients (non-host) will receive the CloudId broadcast message.

			if (hasAuthority) {
				NetworkClient.RegisterHandler<CloudAnchorMessage>(OnCloudIdReceived);
				RequestCloudId();
			}
		}

		Debug.Log($"[ARUser] ARSceneController application mode : {ARSceneController.Instance.applicationMode.ToString()}");
	}

	#region Broadcasting New Cloud Id (Already Joined)
	[Server]
	public void BroadcastCloudId(string cloudId) {
		NetworkServer.SendToAll(new CloudAnchorMessage {
			CloudId = cloudId
		});
	}

	public void OnCloudIdReceived(NetworkConnection conn, CloudAnchorMessage msg) {
		ARSceneController.Instance.SetCloudAnchorId(msg.CloudId);
	}
	#endregion

	#region Requesting Cloud Id (Newly Joined)]
	public void RequestCloudId() {
		CmdRequestCloudId();
	}

	[Command]
	void CmdRequestCloudId() {
		TargetReplyCloudIdRequest(ARSceneController.Instance.AnchorId);
	}

	[TargetRpc]
	public void TargetReplyCloudIdRequest(string serverCloudId) {
		// If server hasn't set the play area yet, (i.e. also no cloud anchor)
		// Try and try again until success.
		if(serverCloudId == "") {
			RetryCloudIdRequest();
			return;
		}

		SetCloudId(serverCloudId);
	}
	
	public void SetCloudId(string CloudId) {
		ARSceneController.Instance.SetCloudAnchorId(CloudId);
	}

	public void RetryCloudIdRequest() {
		StartCoroutine(WaitAndTryRequestCloudId(4f));
	}

	private IEnumerator WaitAndTryRequestCloudId(float waitInterval) {
		yield return new WaitForSeconds(waitInterval);
		RequestCloudId();
	}
	#endregion


}
