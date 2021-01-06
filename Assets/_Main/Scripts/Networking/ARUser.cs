using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

		if (!hasAuthority)
			return;

		username = PlayerPrefs.GetString("username");
		Debug.Log($"[ARUser] Setting username: {username}");
			
		PlayAreaManager.Instance.localUser = this;
		isHost = isServer;

		Debug.Log($"[ARUser] Am I host?: {isHost}");
		if (isHost) {
			ARSceneController.Instance.applicationMode = ApplicationMode.Host;
		}
		else {
			ARSceneController.Instance.applicationMode = ApplicationMode.Client;
			// Only clients (non-host) will receive the CloudId broadcast message.
			NetworkClient.RegisterHandler<CloudAnchorMessage>(OnCloudIdReceived);
			RequestCloudId();
		}

		Debug.Log($"[ARUser] ARSceneController application mode : {ARSceneController.Instance.applicationMode.ToString()}");
	}

	private void OnApplicationQuit() {
		if (hasAuthority)
			NetworkClient.Disconnect();

	}

	#region Broadcasting New Cloud Id (Already Joined)
	[Server]
	public void BroadcastCloudId(string cloudId) {
		Debug.Log($"[{this.GetType().Name}] Broadcasting Cloud Id: {cloudId} ");
		NetworkServer.SendToAll(new CloudAnchorMessage {
			CloudId = cloudId
		});
	}

	public void OnCloudIdReceived(NetworkConnection conn, CloudAnchorMessage msg) {
		Debug.Log($"[{this.GetType().Name}] Received Cloud Id: {msg.CloudId} ");
		ARSceneController.Instance.SetCloudAnchorId(msg.CloudId);
	}
	#endregion

	#region Requesting Cloud Id (Newly Joined)]
	public void RequestCloudId() {
		Debug.Log($"[{this.GetType().Name}] Requesting Cloud Id...");
		CmdRequestCloudId();
	}

	[Command]
	void CmdRequestCloudId() {
		Debug.Log($"[{this.GetType().Name}] Cloud Id Command Received, sending TargetRpc...");
		TargetReplyCloudIdRequest(ARSceneController.Instance.AnchorId);
	}

	[TargetRpc]
	public void TargetReplyCloudIdRequest(string serverCloudId) {
		// If server hasn't set the play area yet, (i.e. also no cloud anchor)
		// Try and try again until success.
		if(serverCloudId == "") {
			Debug.Log($"[{this.GetType().Name}] ServerCloudId is currently empty. Retrying...");
			RetryCloudIdRequest();
			return;
		}


		Debug.Log($"[{this.GetType().Name}] Setting Cloud Id: {serverCloudId}");
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
		Debug.Log($"[{this.GetType().Name}] Wait Interval for retry passed, retrying now...");
		RequestCloudId();
	}
	#endregion


}
